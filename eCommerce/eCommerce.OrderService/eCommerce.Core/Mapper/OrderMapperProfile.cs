using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using eCommerce.BusinessLogicLayer.DTOs;
using eCommerce.DataAccessLayer.Entities;

namespace eCommerce.BusinessLogicLayer.Mapper
{
    public static class OrderMapperProfile
    {
        public static Order ToDo(this OrderAddRequest request)
        {
            return new Order
            {
                OrderID = Guid.NewGuid(),
                UserID = request.UserID,
                OrderDate = request.OrderDate,
                OrderItems = request.OrderItems.Select(x => x.ToDo()).ToList()
            };
        }

        public static OrderItem ToDo(this OrderItemAddRequest request)
        {
            return new OrderItem
            {
                ProductID = request.ProductID,
                Quantity = request.Quantity,
              
                
            };
        }

        public static OrderResponse ToDo(this Order order)
        {
            return new OrderResponse
            {
                OrderID = order.OrderID,
                UserID = order.UserID,
                OrderDate = order.OrderDate,
                OrderItems = order.OrderItems.Select(x => x.ToDo()).ToList(),
                TotalBill = order.TotalBill,
                
            };
        }

        public static OrderItemResponse ToDo(this OrderItem request)
        {
            return new OrderItemResponse
            {
                ProductID = request.ProductID,
                Quantity = request.Quantity,
                UnitPrice = request.UnitPrice,
                TotalPrice = request.TotalPrice,
               
            };
        }

        public static Order ToDo(this OrderUpdateRequest request)
        {
            return new Order
            {
                OrderID = request.OrderID,
                UserID = request.UserID,
                OrderDate = request.OrderDate,
                OrderItems = request.OrderItems.Select(x => x.ToDo()).ToList()
            };
        }

        public static OrderItem ToDo(this OrderItemUpdateRequest request)
        {
            return new OrderItem
            {
                ProductID = request.ProductID,
                Quantity = request.Quantity,
                
            };
        }

    }
}
