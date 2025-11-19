using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using eCommerce.BusinessLogicLayer.RabbitMQ;
using eCommerce.DataAccessLayer.Repository;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace eCommerce.Core.RabbitMQ
{
    public class RabbitMQOrderDeletedConsumer : IDisposable, IRabbitMQOrderDeletedConsumer
    {
        private readonly IConfiguration _configuration;
        private readonly IChannel _channel;
        private readonly IConnection _connection;
        private readonly ILogger<RabbitMQOrderDeletedConsumer> _logger;
        private readonly IProductRepo _productRepo;

        public RabbitMQOrderDeletedConsumer(
            IConfiguration configuration, 
            ILogger<RabbitMQOrderDeletedConsumer> logger,
            IProductRepo productRepo)
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
            _productRepo = productRepo;

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
                { "event", "order.deleted" },
                { "RowCount", 1 }
            };

            string queueName = "products.order.deleted.queue";
            string exchangeName = Environment.GetEnvironmentVariable("RabbitMQ_Orders_Exchange") 
                ?? _configuration["RabbitMQ_Orders_Exchange"] 
                ?? "order.exchange";

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

                    if (message != null)
                    {
                        OrderDeletedMessage? orderDeletedMessage = JsonSerializer.Deserialize<OrderDeletedMessage>(message);

                        if (orderDeletedMessage != null)
                        {
                            _logger.LogInformation($"Order deleted: {orderDeletedMessage.OrderID}, User: {orderDeletedMessage.UserID}");
                            await HandleOrderDeleted(orderDeletedMessage);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing order deleted message: {ex.Message}");
                }
            };

            await _channel.BasicConsumeAsync(queue: queueName, consumer: consumer, autoAck: true);
            _logger.LogInformation($"Started consuming order.deleted events from queue: {queueName}");
        }

        private async Task HandleOrderDeleted(OrderDeletedMessage message)
        {
            var orderItems = message.OrderItems ?? Enumerable.Empty<OrderItemMessage>();

            foreach (var orderItem in orderItems)
            {
                // Increase stock by the ordered quantity (positive value increases)
                var result = await _productRepo.UpdateProductStock(orderItem.ProductID, orderItem.Quantity);
                
                if (result.IsSuccess)
                {
                    _logger.LogInformation($"Restored stock for product {orderItem.ProductID} by {orderItem.Quantity}");
                }
                else
                {
                    _logger.LogWarning($"Failed to restore stock for product {orderItem.ProductID}: {string.Join(", ", result.Errors)}");
                }
            }
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}

