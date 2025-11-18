using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Amazon.Runtime.Internal.Util;

using eCommerce.BusinessLogicLayer.DTOs;
using eCommerce.BusinessLogicLayer.HttpClientt;
using eCommerce.BusinessLogicLayer.Mapper;
using eCommerce.BusinessLogicLayer.RabbitMQ;
using eCommerce.BusinessLogicLayer.Validator;
using eCommerce.DataAccessLayer.Entities;
using eCommerce.DataAccessLayer.Repository;
using eCommerce.Shared.DTOs;

using FluentResults;

using FluentValidation;

using Microsoft.Extensions.Logging;

using MongoDB.Driver;

namespace eCommerce.BusinessLogicLayer.Services
{
    public class OrderService(IValidator<OrderAddRequest> Orderaddvalidator ,IValidator<OrderItemAddRequest> orderItemAddValidator, IValidator<OrderUpdateRequest> orderUpdateValidator, IValidator<OrderItemUpdateRequest> orderItemUpdateValidator, IOrderRepository _repo , ProductsMicroserviceClient productsMicroserviceClient, UsersMicroserviceClient usersMicroserviceClient, IRabbitMqPublisher rabbitMqPublisher , ILogger<OrderService>_logger) : IOrderService
    {
        public async Task<Result<OrderResponse>> AddOrder(OrderAddRequest request)
        {
            var validator =await Orderaddvalidator.ValidateAsync(request);
            if (!validator.IsValid)
            {
                var errors = string.Join(", ", validator.Errors.Select(x => x.ErrorMessage));
                return Result.Fail(errors);
            }

            List<ProductDTO> products = new List<ProductDTO>();

            foreach (var item in request.OrderItems)
            {
                var Itemvalidator = await orderItemAddValidator.ValidateAsync(item);
                if (!Itemvalidator.IsValid)
                {
                    string errors = string.Join(", ", Itemvalidator.Errors.Select(x => x.ErrorMessage));
                    return Result.Fail(errors);
                }
                // checking if Product Exist or Not ???   using HttpClient 
                var product = await productsMicroserviceClient.GetProductByID(item.ProductID);
                if(product == null)
                {
                    return Result.Fail("Invalid ProductID");
                }
                
                products.Add(product.Value.Data);
            }

            // get user 
            var user = await usersMicroserviceClient.GetUserById(request.UserID);
            if(user.Value == null)
            {
                return Result.Fail("User Id Invalid");
            }
            Order orderInput = request.ToDo();
            // calculate total bill to save on order entity
            foreach (var order in orderInput.OrderItems)
            {
               var p = products.FirstOrDefault(x => x.ProductID  == order.ProductID);
                order.UnitPrice = p.UnitPrice;
                order.Category = p.Category;
                order.ProductName = p.ProductName;
                order.TotalPrice = order.Quantity * order.UnitPrice;
                _logger.LogInformation($"{order.ProductID} = {order.TotalPrice} -> q {order.Quantity} - {order.UnitPrice}");
            }
            orderInput.TotalBill = orderInput.OrderItems.Sum(x => x.TotalPrice);
            _logger.LogInformation($"{orderInput.TotalBill}---> bill");

            var res = await _repo.AddOrder(orderInput);
            if (res.IsSuccess)
            {
                // Publish order created event to RabbitMQ
                var orderCreatedMessage = new OrderCreatedMessage(
                    res.Value.OrderID,
                    res.Value.UserID,
                    res.Value.TotalBill,
                    res.Value.OrderDate
                );
                
                var headers = new Dictionary<string, object>()
                {
                    { "event", "order.created" },
                    { "RowCount", 1 }
                };
                
                await rabbitMqPublisher.Publish(headers, orderCreatedMessage);
                
                return Result.Ok(res.Value.ToDo() with { Email = user.Value.Data.Email , UserName = user.Value.Data.PersonName });
            }
            return Result.Fail(res.Errors);
        }

        public async Task<Result<bool>> Delete(Guid requestId)
        {
            var result = await _repo.DeleteOrder(requestId);
            if (result.IsSuccess)
            {
                // Publish order deleted event to RabbitMQ
                var orderDeletedMessage = new OrderDeletedMessage(
                    result.Value.OrderID,
                    result.Value.UserID
                );
                
                var headers = new Dictionary<string, object>()
                {
                    { "event", "order.deleted" },
                    { "RowCount", 1 }
                };
                
                await rabbitMqPublisher.Publish(headers, orderDeletedMessage);
                
                return Result.Ok(true);
            }
            return Result.Fail(result.Errors);
        }

        public async Task<Result<OrderResponse>> GetOrderByCondition(FilterDefinition<Order> filter)
        {
            var result = await _repo.GetOrderByFilter(filter);
            if (!result.IsSuccess || result.Value == null)
            {
                return Result.Fail(result.Errors);
            }

            var order = result.Value;
            var user = await usersMicroserviceClient.GetUserById(order.UserID);
            
            var orderResponse = order.ToDo();
            if (user.IsSuccess && user.Value != null)
            {
                orderResponse = orderResponse with { Email = user.Value.Data.Email, UserName = user.Value.Data.PersonName };
            }

            // Enrich order items with product information
            var enrichedOrderItems = new List<OrderItemResponse>();
            foreach (var item in order.OrderItems)
            {
                var product = await productsMicroserviceClient.GetProductByID(item.ProductID);
                var itemResponse = item.ToDo();
                if (product.IsSuccess && product.Value != null)
                {
                    itemResponse = itemResponse with { ProductName = product.Value.Data.ProductName, Category = product.Value.Data.Category };
                }
                enrichedOrderItems.Add(itemResponse);
            }
            orderResponse = orderResponse with { OrderItems = enrichedOrderItems };

            return Result.Ok(orderResponse);
        }

        public async Task<Result<PagedList<OrderResponse>>> GetOrders(int pageNumber, int pageSize)
        {
            var result = await _repo.GetOrders( pageNumber,  pageSize);
            if (!result.IsSuccess)
            {
                return Result.Fail(result.Errors);
            }

            var orders = result.Value;
            var orderResponses = new List<OrderResponse>();

            foreach (var order in orders.Items)
            {
                var user = await usersMicroserviceClient.GetUserById(order.UserID);
                var orderResponse = order.ToDo();
                
                if (user.IsSuccess && user.Value != null)
                {
                    orderResponse = orderResponse with { Email = user.Value.Data.Email, UserName = user.Value.Data.PersonName };
                }

                // Enrich order items with product information
                var enrichedOrderItems = new List<OrderItemResponse>();
                foreach (var item in order.OrderItems)
                {
                    var product = await productsMicroserviceClient.GetProductByID(item.ProductID);
                    var itemResponse = item.ToDo();
                    if (product.IsSuccess && product.Value != null)
                    {
                        itemResponse = itemResponse with { ProductName = product.Value.Data.ProductName, Category = product.Value.Data.Category };
                    }
                    enrichedOrderItems.Add(itemResponse);
                }
                orderResponse = orderResponse with { OrderItems = enrichedOrderItems };
                orderResponses.Add(orderResponse);
            }

            return Result.Ok(new PagedList<OrderResponse>(orderResponses.ToList() , result.Value.TotalCount , pageNumber , pageSize));
        }

        public async Task<Result<PagedList<OrderResponse>>> GetOrdersByCondition(FilterDefinition<Order> filter , int pageNumber, int pageSize)
        {
            var result = await _repo.GetOrdersByFilter(filter ,  pageNumber,  pageSize);
            if (!result.IsSuccess)
            {
                return Result.Fail(result.Errors);
            }

            var orders = result.Value;
            var orderResponses = new List<OrderResponse>();

            foreach (var order in orders.Items)
            {
                var user = await usersMicroserviceClient.GetUserById(order.UserID);
                var orderResponse = order.ToDo();
                
                if (user.IsSuccess && user.Value != null)
                {
                    orderResponse = orderResponse with { Email = user.Value.Data.Email, UserName = user.Value.Data.PersonName };
                }

                // Enrich order items with product information
                var enrichedOrderItems = new List<OrderItemResponse>();
                foreach (var item in order.OrderItems)
                {
                    var product = await productsMicroserviceClient.GetProductByID(item.ProductID);
                    var itemResponse = item.ToDo();
                    if (product.IsSuccess && product.Value != null)
                    {
                        itemResponse = itemResponse with { ProductName = product.Value.Data.ProductName, Category = product.Value.Data.Category };
                    }
                    enrichedOrderItems.Add(itemResponse);
                }
                orderResponse = orderResponse with { OrderItems = enrichedOrderItems };
                orderResponses.Add(orderResponse);
            }

            return Result.Ok(new PagedList<OrderResponse>(orderResponses.ToList(), result.Value.TotalCount, pageNumber, pageSize));

        }

        public async Task<Result<OrderResponse>> UpdateOrder(OrderUpdateRequest request)
        {
            var validator = await orderUpdateValidator.ValidateAsync(request);
            if (!validator.IsValid)
            {
                var errors = string.Join(", ", validator.Errors.Select(x => x.ErrorMessage));
                return Result.Fail(errors);
            }

            // Validate order items
            foreach (var item in request.OrderItems)
            {
                var itemValidator = await orderItemUpdateValidator.ValidateAsync(item);
                if (!itemValidator.IsValid)
                {
                    string errors = string.Join(", ", itemValidator.Errors.Select(x => x.ErrorMessage));
                    return Result.Fail(errors);
                }
                
                // Check if product exists
                var product = await productsMicroserviceClient.GetProductByID(item.ProductID);
                if (product == null || !product.IsSuccess)
                {
                    return Result.Fail($"Invalid ProductID: {item.ProductID}");
                }
            }

            // Check if user exists
            var user = await usersMicroserviceClient.GetUserById(request.UserID);
            if (user.Value == null)
            {
                return Result.Fail("User Id Invalid");
            }

            // Check if order exists
            var existingOrderResult = await _repo.GetOrderByFilter(Builders<Order>.Filter.Eq(x => x.OrderID, request.OrderID));
            if (!existingOrderResult.IsSuccess || existingOrderResult.Value == null)
            {
                return Result.Fail("Order not found");
            }

            var existingOrder = existingOrderResult.Value;
            var orderInput = request.ToDo();
            orderInput.ID = existingOrder.ID; // Preserve the ID

            // Calculate total bill
            foreach (var orderItem in orderInput.OrderItems)
            {
                orderItem.TotalPrice = orderItem.Quantity * orderItem.UnitPrice;
                if (orderItem._id == Guid.Empty)
                {
                    orderItem._id = Guid.NewGuid();
                }
            }
            orderInput.TotalBill = orderInput.OrderItems.Sum(x => x.TotalPrice);

            var updateResult = await _repo.UpdateOrder(orderInput);
            if (!updateResult.IsSuccess)
            {
                return Result.Fail(updateResult.Errors);
            }

            var updatedOrder = updateResult.Value;
            
            // Publish order updated event to RabbitMQ
            var orderUpdatedMessage = new OrderUpdatedMessage(
                updatedOrder.OrderID,
                updatedOrder.UserID,
                updatedOrder.TotalBill,
                updatedOrder.OrderDate
            );
            
            var headers = new Dictionary<string, object>()
            {
                { "event", "order.updated" },
                { "RowCount", 1 }
            };
            
            await rabbitMqPublisher.Publish(headers, orderUpdatedMessage);
            
            var orderResponse = updatedOrder.ToDo() with { Email = user.Value.Data.Email, UserName = user.Value.Data.PersonName };

            // Enrich order items with product information
            var enrichedOrderItems = new List<OrderItemResponse>();
            foreach (var item in updatedOrder.OrderItems)
            {
                var product = await productsMicroserviceClient.GetProductByID(item.ProductID);
                var itemResponse = item.ToDo();
                if (product.IsSuccess && product.Value != null)
                {
                    itemResponse = itemResponse with { ProductName = product.Value.Data.ProductName, Category = product.Value.Data.Category };
                }
                enrichedOrderItems.Add(itemResponse);
            }
            orderResponse = orderResponse with { OrderItems = enrichedOrderItems };

            return Result.Ok(orderResponse);
        }
    }
}
