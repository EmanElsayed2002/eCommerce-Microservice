using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using eCommerce.BusinessLogicLayer.DTOs;
using eCommerce.DataAccessLayer.Entities;
using eCommerce.Shared.DTOs;

using FluentResults;

using MongoDB.Driver;

namespace eCommerce.BusinessLogicLayer.Services
{
    public interface IOrderService
    {
        Task<Result<PagedList<OrderResponse>>> GetOrders(int pageNumber , int pageSize);
        Task<Result<PagedList<OrderResponse>>> GetOrdersByCondition(FilterDefinition<Order> filter , int pageNumber, int pageSize);
        Task<Result<OrderResponse>> GetOrderByCondition(FilterDefinition<Order> filter);
        Task<Result<OrderResponse>> AddOrder(OrderAddRequest request);
        Task<Result<OrderResponse>> UpdateOrder(OrderUpdateRequest request);
        Task<Result<bool>> Delete(Guid requestId);
    }
}
