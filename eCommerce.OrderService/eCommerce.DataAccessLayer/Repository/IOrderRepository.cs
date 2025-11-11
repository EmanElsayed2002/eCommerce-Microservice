using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using eCommerce.DataAccessLayer.Entities;

using FluentResults;

using MongoDB.Driver;

namespace eCommerce.DataAccessLayer.Repository
{
    public interface IOrderRepository
    {
        Task<Result<IEnumerable<Order>>> GetOrders();
        Task<Result<IEnumerable<Order>>> GetOrdersByConditions(Expression<Func<Order, bool>> predicate);
        Task<Result<IEnumerable<Order>>> GetOrdersByFilter(FilterDefinition<Order> filter);
        Task<Result<Order>> GetOrderByCondition(Expression<Func<Order, bool>> predicate);
        Task<Result<Order>> GetOrderByFilter(FilterDefinition<Order> filter);
        Task<Result<Order>> AddOrder(Order order);
        Task<Result<Order>> UpdateOrder(Order order);
        Task<Result<Order>> DeleteOrder(Guid orderId);


    }
}
