using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using eCommerce.DataAccessLayer.Entities;
using eCommerce.DataAccessLayer.Repository;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using MongoDB.Driver;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace eCommerce.BusinessLogicLayer.RabbitMQ
{
    public class RabbitMQProductDeletionConsumer : IDisposable, IRabbitMQProductDeletionConsumer
    {
        private readonly IConfiguration _configuration;
        private readonly IChannel _channel;
        private readonly IConnection _connection;
        private readonly ILogger<RabbitMQProductDeletionConsumer> _logger;
        private readonly IDistributedCache _cache;
        private readonly IOrderRepository _orderRepository;

        public RabbitMQProductDeletionConsumer(
            IConfiguration configuration, 
            ILogger<RabbitMQProductDeletionConsumer> logger, 
            IDistributedCache cache,
            IOrderRepository orderRepository)
        {
            _configuration = configuration;
            
            string hostName = Environment.GetEnvironmentVariable("RabbitMQ_HostName") 
                ?? _configuration["RabbitMQ_HostName"] 
                ?? throw new ArgumentNullException("RabbitMQ_HostName configuration is missing");
            
            string userName = Environment.GetEnvironmentVariable("RabbitMQ_UserName") 
                ?? _configuration["RabbitMQ_UserName"] 
                ?? throw new ArgumentNullException("RabbitMQ_UserName configuration is missing");
            
            string password = Environment.GetEnvironmentVariable("RabbitMQ_Password") 
                ?? _configuration["RabbitMQ_Password"] 
                ?? throw new ArgumentNullException("RabbitMQ_Password configuration is missing");
            
            string port = Environment.GetEnvironmentVariable("RabbitMQ_Port") 
                ?? _configuration["RabbitMQ_Port"] 
                ?? throw new ArgumentNullException("RabbitMQ_Port configuration is missing");

            _logger = logger;
            _cache = cache;
            _orderRepository = orderRepository;

            ConnectionFactory connectionFactory = new ConnectionFactory()
            {
                HostName = hostName.Trim(),
                UserName = userName.Trim(),
                Password = password.Trim(),
                Port = Convert.ToInt32(port)
            };

            _connection = connectionFactory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
        }

        public async Task Consume()
        {
            var headers = new Dictionary<string, object>()
            {
                { "x-match", "all" },
                { "event", "product.delete" },
                { "RowCount", 1 }
            };

            string queueName = "orders.product.delete.queue";
            string exchangeName = Environment.GetEnvironmentVariable("RabbitMQ_Products_Exchange") 
                ?? _configuration["RabbitMQ_Products_Exchange"] 
                ?? throw new InvalidOperationException("RabbitMQ_Products_Exchange configuration is missing");

            // Create exchange (Headers type for header-based routing)
           await _channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Headers, durable: true);

            // Create message queue
           await _channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            // Bind the queue to exchange using headers
           await _channel.QueueBindAsync(queue: queueName, exchange: exchangeName, routingKey: string.Empty, arguments: headers);

            AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (sender, args) =>
            {
                try
                {
                    byte[] body = args.Body.ToArray();
                    string message = Encoding.UTF8.GetString(body);

                    _logger.LogInformation("Received product deletion message: {Message}", message);

                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };

                        ProductDeleteMessage? productDeleteMessage = JsonSerializer.Deserialize<ProductDeleteMessage>(message, options);

                        if (productDeleteMessage != null)
                        {
                            _logger.LogInformation("Product deletion event received: ProductId={ProductId}, ProductName={ProductName}", 
                                productDeleteMessage.ProductId, 
                                productDeleteMessage.ProductName);
                            await HandleProductDeletion(productDeleteMessage.ProductId);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to deserialize product deletion message");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing product deletion message: {Error}", ex.Message);
                }
            };

          await  _channel.BasicConsumeAsync(queue: queueName, consumer: consumer, autoAck: true);
        }

        private async Task HandleProductDeletion(Guid productID)
        {
            _logger.LogInformation("Processing product deletion: ProductId={ProductId}", productID);

            // Find all orders containing this product
            var filter = Builders<Order>.Filter.ElemMatch(
                o => o.OrderItems,
                Builders<OrderItem>.Filter.Eq(oi => oi.ProductID, productID)
            );

            var ordersResult = await _orderRepository.GetOrdersByFilter(filter, 1, 1000);
            
            if (ordersResult.IsSuccess && ordersResult.Value != null)
            {
                var orders = ordersResult.Value.Items;
                _logger.LogInformation("Found {Count} orders containing deleted product {ProductId}", 
                    orders.Count, 
                    productID);

                foreach (var order in orders)
                {
                    // Remove order items with the deleted product
                    var itemsToRemove = order.OrderItems.Where(oi => oi.ProductID == productID).ToList();
                    
                    if (itemsToRemove.Any())
                    {
                        foreach (var item in itemsToRemove)
                        {
                            order.OrderItems.Remove(item);
                        }

                        // Recalculate total bill
                        order.TotalBill = order.OrderItems.Sum(oi => oi.TotalPrice);

                        var updateResult = await _orderRepository.UpdateOrder(order);
                        if (updateResult.IsSuccess)
                        {
                            _logger.LogInformation("Removed {Count} order items from order {OrderId} for deleted product {ProductId}. New total: {TotalBill}", 
                                itemsToRemove.Count, 
                                order.OrderID, 
                                productID,
                                order.TotalBill);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to update order {OrderId}: {Errors}", 
                                order.OrderID, 
                                string.Join(", ", updateResult.Errors));
                        }
                    }
                }
            }

            // Remove product from cache when it's deleted
            // Note: Cache key format matches ProductsMicroserviceClient: "product: {productID}"
            string cacheKeyToWrite = $"product: {productID}";
            await _cache.RemoveAsync(cacheKeyToWrite);
            _logger.LogInformation("Removed product {ProductId} from cache", productID);
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}

