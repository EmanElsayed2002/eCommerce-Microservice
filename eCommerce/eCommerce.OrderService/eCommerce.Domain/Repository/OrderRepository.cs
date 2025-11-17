using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using eCommerce.DataAccessLayer.Entities;
using eCommerce.Shared.DTOs;

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

        public async Task<Result<PagedList<Order>>> GetOrders(int pageNumber , int pageSize)
        {
            var counts = (await orders.FindAsync(Builders<Order>.Filter.Empty)).ToList();
            return new PagedList<Order>(counts, counts.Count, pageNumber, pageSize);
            

          
        }

        public async Task<Result<PagedList<Order>>> GetOrdersByConditions(Expression<Func<Order, bool>> predicate, int pageNumber, int pageSize)
        {
            var counts = (await orders.FindAsync(predicate)).ToList();
            return new PagedList<Order>(counts, counts.Count, pageNumber, pageSize);
        }

        public async Task<Result<PagedList<Order>>> GetOrdersByFilter(FilterDefinition<Order> filter , int pageNumber, int pageSize)
        {
            var counts = (await orders.FindAsync(filter)).ToList();
            return new PagedList<Order>(counts, counts.Count, pageNumber, pageSize);
           
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
