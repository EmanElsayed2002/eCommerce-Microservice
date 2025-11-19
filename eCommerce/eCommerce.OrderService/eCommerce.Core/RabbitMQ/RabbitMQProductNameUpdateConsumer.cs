using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using eCommerce.BusinessLogicLayer.DTOs;
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
    public class RabbitMQProductNameUpdateConsumer : IDisposable, IRabbitMQProductNameUpdateConsumer
    {
        private readonly IConfiguration _configuration;
        private readonly IChannel _channel;
        private readonly IConnection _connection;
        private readonly ILogger<RabbitMQProductNameUpdateConsumer> _logger;
        private readonly IDistributedCache _cache;
        private readonly IOrderRepository _orderRepository;

        public RabbitMQProductNameUpdateConsumer(
            IConfiguration configuration, 
            ILogger<RabbitMQProductNameUpdateConsumer> logger, 
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
                { "event", "product.update" },
                { "RowCount", 1 }
            };

            string queueName = "orders.product.update.name.queue";
            string exchangeName = Environment.GetEnvironmentVariable("RabbitMQ_Products_Exchange") 
                ?? _configuration["RabbitMQ_Products_Exchange"] 
                ?? throw new InvalidOperationException("RabbitMQ_Products_Exchange configuration is missing");

            // Create exchange (Headers type for header-based routing)
           await _channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Headers, durable: true);

            // Create message queue
           await _channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            // Bind the queue to exchange using headers
          await  _channel.QueueBindAsync(queue: queueName, exchange: exchangeName, routingKey: string.Empty, arguments: headers);

            AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (sender, args) =>
            {
                try
                {
                    byte[] body = args.Body.ToArray();
                    string message = Encoding.UTF8.GetString(body);

                    _logger.LogInformation("Received product update message: {Message}", message);

                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };

                        ProductUpdateMessage? productUpdateMessage = JsonSerializer.Deserialize<ProductUpdateMessage>(message, options);

                        if (productUpdateMessage != null)
                        {
                            _logger.LogInformation("Product update event received: ProductId={ProductId}, NewName={NewName}", 
                                productUpdateMessage.ProductId, 
                                productUpdateMessage.NewName);
                            await HandleProductUpdation(productUpdateMessage);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to deserialize product update message");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing product update message: {Error}", ex.Message);
                }
            };

           await _channel.BasicConsumeAsync(queue: queueName, consumer: consumer, autoAck: true);
        }

        private async Task HandleProductUpdation(ProductUpdateMessage productUpdateMessage)
        {
            _logger.LogInformation("Processing product name update: ProductId={ProductId}, NewName={NewName}", 
                productUpdateMessage.ProductId, 
                productUpdateMessage.NewName);

            // Find all orders containing this product
            var filter = Builders<Order>.Filter.ElemMatch(
                o => o.OrderItems,
                Builders<OrderItem>.Filter.Eq(oi => oi.ProductID, productUpdateMessage.ProductId)
            );

            var ordersResult = await _orderRepository.GetOrdersByFilter(filter, 1, 1000);
            
            if (ordersResult.IsSuccess && ordersResult.Value != null)
            {
                var orders = ordersResult.Value.Items;
                _logger.LogInformation("Found {Count} orders containing product {ProductId}", 
                    orders.Count, 
                    productUpdateMessage.ProductId);

                foreach (var order in orders)
                {
                    bool updated = false;
                    foreach (var orderItem in order.OrderItems)
                    {
                        if (orderItem.ProductID == productUpdateMessage.ProductId)
                        {
                            if (!string.IsNullOrWhiteSpace(productUpdateMessage.NewName))
                            {
                                orderItem.ProductName = productUpdateMessage.NewName;
                                updated = true;
                            }
                        }
                    }

                    if (updated)
                    {
                        var updateResult = await _orderRepository.UpdateOrder(order);
                        if (updateResult.IsSuccess)
                        {
                            _logger.LogInformation("Updated order {OrderId} with new product name for product {ProductId}", 
                                order.OrderID, 
                                productUpdateMessage.ProductId);
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

            // Update cache with new product information
            // Note: Cache key format matches ProductsMicroserviceClient: "product: {productID}"
            if (!string.IsNullOrWhiteSpace(productUpdateMessage.NewName))
            {
                string cacheKeyToWrite = $"product: {productUpdateMessage.ProductId}";
                await _cache.RemoveAsync(cacheKeyToWrite);
                _logger.LogInformation("Removed product {ProductId} from cache (will be refreshed on next request)", 
                    productUpdateMessage.ProductId);
            }
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}

