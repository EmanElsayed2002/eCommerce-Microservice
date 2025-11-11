using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using eCommerce.BusinessLogicLayer.DTOs;
using eCommerce.DataAccessLayer.Entities;

using FluentResults;

using MongoDB.Driver;

namespace eCommerce.BusinessLogicLayer.Services
{
    public interface IOrderService
    {
        Task<Result<IEnumerable<OrderResponse>>> GetOrders();
        Task<Result<IEnumerable<OrderResponse>>> GetOrdersByCondition(FilterDefinition<Order> filter);
        Task<Result<OrderResponse>> GetOrderByCondition(FilterDefinition<Order> filter);
        Task<Result<OrderResponse>> AddOrder(OrderAddRequest request);
        Task<Result<OrderResponse>> UpdateOrder(OrderUpdateRequest request);
        Task<Result<bool>> Delete(Guid requestId);


    }
}
