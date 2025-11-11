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
    public class OrderRepository : IOrderRepository
    {
        public readonly IMongoCollection<Order> orders;
        public OrderRepository(IMongoDatabase ordersdb)
        {
            orders = ordersdb.GetCollection<Order>("orders");
        }
        public async Task<Result<Order>> AddOrder(Order order)
        {
            order.ID = Guid.NewGuid();
            order.OrderID = order.ID;
            foreach(var item in order.OrderItems)
            {
                item._id = Guid.NewGuid();
            }

            await orders.InsertOneAsync(order);
            return order;
        }

        public async Task<Result<Order>> DeleteOrder(Guid orderId)
        {
            FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.ID ,  orderId);
            Order order =await orders.FindSync(filter).FirstOrDefaultAsync();
            if (order == null) return Result.Fail("Order Not Exist");
            var deleted =await orders.DeleteOneAsync(filter);
            if (deleted.DeletedCount == 0) return Result.Fail("Not Deletd");
            
            
            return Result.Ok(order);

        }

        public async Task<Result<Order>> GetOrderByCondition(Expression<Func<Order, bool>> predicate)
        {
            return (await orders.FindAsync(predicate)).FirstOrDefault();
        }

        public async Task<Result<IEnumerable<Order>>> GetOrders()
        {
            return (await orders.FindAsync(Builders<Order>.Filter.Empty)).ToList();
        }

        public async Task<Result<IEnumerable<Order>>> GetOrdersByConditions(Expression<Func<Order, bool>> predicate)
        {
            return (await orders.FindAsync(predicate)).ToList();
        }

        public async Task<Result<IEnumerable<Order>>> GetOrdersByFilter(FilterDefinition<Order> filter)
        {
            return (await orders.FindAsync(filter)).ToList();
        }

        public async Task<Result<Order>> GetOrderByFilter(FilterDefinition<Order> filter)
        {
            return (await orders.FindAsync(filter)).FirstOrDefault();
        }

        public async Task<Result<Order>> UpdateOrder(Order order)
        {
            FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(x => x.OrderID , order.OrderID);
            var result = (await orders.FindAsync(filter)).FirstOrDefault();
            if (result == null) return Result.Fail("Order Not Exits");
            await orders.ReplaceOneAsync(filter, order);
            return Result.Ok(order);
        }
    }
}
