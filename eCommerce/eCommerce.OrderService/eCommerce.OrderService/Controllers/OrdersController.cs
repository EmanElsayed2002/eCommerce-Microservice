using eCommerce.BusinessLogicLayer.DTOs;
using eCommerce.BusinessLogicLayer.Services;
using eCommerce.DataAccessLayer.Entities;
using eCommerce.Shared.Resolver;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using MongoDB.Driver;

namespace eCommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController(IOrderService _ordersService) : ControllerBase
    {

        public record GetPaginated(int pageNumber = 1, int pageSize = 10);


        //GET: /api/Orders
        [HttpGet]
        public async Task<IActionResult> Get(GetPaginated request)
        {
            return (await _ordersService.GetOrders(request.pageNumber , request.pageSize)).ResolveToIActionResult(successStatusCode: StatusCodes.Status200OK, context: HttpContext, "Successfully Geting Orders");
        }


        //GET: /api/Orders/search/orderid/{orderID}
        [HttpGet("search/orderid/{orderID}")]
        public async Task<IActionResult?> GetOrderByOrderID(Guid orderID)
        {
            FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderID, orderID);

           
            return (await _ordersService.GetOrderByCondition(filter)).ResolveToIActionResult(successStatusCode: StatusCodes.Status200OK, context: HttpContext, "Successfully Geting Orders");

        }


        //GET: /api/Orders/search/productid/{productID}
        [HttpGet("search/productid/{productID}")]
        public async Task<IActionResult> GetOrdersByProductID(Guid productID  , GetPaginated request)
        {
            FilterDefinition<Order> filter = Builders<Order>.Filter.ElemMatch(temp => temp.OrderItems,
              Builders<OrderItem>.Filter.Eq(tempProduct => tempProduct.ProductID, productID)
              );
           
            return (await _ordersService.GetOrdersByCondition(filter , request.pageNumber, request.pageSize)).ResolveToIActionResult(successStatusCode: StatusCodes.Status200OK, context: HttpContext, "Successfully Geting Orders");

        }


        //GET: /api/Orders/search/orderDate/{orderDate}
        [HttpGet("search/orderDate/{orderDate}")]
        public async Task<IActionResult> GetOrdersByOrderDate(DateTime orderDate , GetPaginated request)
        {
            FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderDate.ToString("yyyy-MM-dd"), orderDate.ToString("yyyy-MM-dd")
              );

            
            return (await _ordersService.GetOrdersByCondition(filter , request.pageNumber, request.pageSize)).ResolveToIActionResult(successStatusCode: StatusCodes.Status200OK, context: HttpContext, "Successfully Geting Orders");

        }


        //GET: /api/Orders/search/userid/{userID}
        [HttpGet("search/userid/{userID}")]
        public async Task<IActionResult> GetOrdersByUserID(Guid userID , GetPaginated request)
        {
            FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.UserID, userID);

            return (await _ordersService.GetOrdersByCondition(filter , request.pageNumber, request.pageSize)).ResolveToIActionResult(successStatusCode: StatusCodes.Status200OK, context: HttpContext, "Successfully Geting Orders");

        }


        //POST api/Orders
        [HttpPost]
        public async Task<IActionResult> Post(OrderAddRequest orderAddRequest)
        {
            if (orderAddRequest == null)
            {
                return BadRequest("Invalid order data");
            }

           
            return (await _ordersService.AddOrder(orderAddRequest)).ResolveToIActionResult(successStatusCode: StatusCodes.Status200OK, context: HttpContext, "Successfully Geting Orders");

        }


        //PUT api/Orders/{orderID}
        [HttpPut("{orderID}")]
        public async Task<IActionResult> Put(Guid orderID, OrderUpdateRequest orderUpdateRequest)
        {
           
            return (await _ordersService.UpdateOrder(orderUpdateRequest)).ResolveToIActionResult(successStatusCode: StatusCodes.Status200OK, context: HttpContext, "Successfully Geting Orders");

        }


        //DELETE api/Orders/{orderID}
        [HttpDelete("{orderID}")]
        public async Task<IActionResult> Delete(Guid orderID)
        {
            if (orderID == Guid.Empty)
            {
                return BadRequest("Invalid order ID");
            }

            return (await _ordersService.Delete(orderID)).ResolveToIActionResult(successStatusCode: StatusCodes.Status200OK, context: HttpContext, "Successfully Geting Orders");

        }
    }
}
