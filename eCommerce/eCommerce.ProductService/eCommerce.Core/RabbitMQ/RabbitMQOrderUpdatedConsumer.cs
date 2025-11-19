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
    public class RabbitMQOrderUpdatedConsumer : IDisposable, IRabbitMQOrderUpdatedConsumer
    {
        private readonly IConfiguration _configuration;
        private readonly IChannel _channel;
        private readonly IConnection _connection;
        private readonly ILogger<RabbitMQOrderUpdatedConsumer> _logger;
        private readonly IProductRepo _productRepo;

        public RabbitMQOrderUpdatedConsumer(
            IConfiguration configuration,
            ILogger<RabbitMQOrderUpdatedConsumer> logger,
            IProductRepo productRepo)
        {
            _configuration = configuration;
            _logger = logger;
            _productRepo = productRepo;

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
            var headers = new Dictionary<string, object>
            {
                { "x-match", "all" },
                { "event", "order.updated" },
                { "RowCount", 1 }
            };

            string queueName = "products.order.updated.queue";
            string exchangeName = Environment.GetEnvironmentVariable("RabbitMQ_Orders_Exchange")
                ?? _configuration["RabbitMQ_Orders_Exchange"]
                ?? "order.exchange";

            await _channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Headers, durable: true);
            await _channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            await _channel.QueueBindAsync(queue: queueName, exchange: exchangeName, routingKey: string.Empty, arguments: headers);

            AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (sender, args) =>
            {
                try
                {
                    byte[] body = args.Body.ToArray();
                    string message = Encoding.UTF8.GetString(body);

                    _logger.LogInformation("Received message: {Message}", message);

                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };
                        
                        OrderUpdatedMessage? orderUpdatedMessage = JsonSerializer.Deserialize<OrderUpdatedMessage>(message, options);
                        if (orderUpdatedMessage != null)
                        {
                            _logger.LogInformation("Order updated event received for Order {OrderId} with {Count} item changes", 
                                orderUpdatedMessage.OrderID, 
                                orderUpdatedMessage.ItemChanges?.Count() ?? 0);
                            await HandleOrderUpdated(orderUpdatedMessage);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to deserialize order updated message");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing order updated message: {Error}", ex.Message);
                }
            };

            await _channel.BasicConsumeAsync(queue: queueName, consumer: consumer, autoAck: true);
            _logger.LogInformation("Started consuming order.updated events from queue: {QueueName}", queueName);
        }

        private async Task HandleOrderUpdated(OrderUpdatedMessage message)
        {
            var itemChanges = message.ItemChanges ?? Enumerable.Empty<OrderItemDeltaMessage>();
            
            _logger.LogInformation("Processing {Count} item changes for order {OrderId}", 
                itemChanges.Count(), 
                message.OrderID);

            if (!itemChanges.Any())
            {
                _logger.LogInformation("No item changes to process for order {OrderId}", message.OrderID);
                return;
            }

            foreach (var change in itemChanges)
            {
                _logger.LogInformation("Processing change for product {ProductId}: QuantityChange = {QuantityChange}", 
                    change.ProductID, 
                    change.QuantityChange);

                if (change.QuantityChange == 0)
                {
                    _logger.LogInformation("Skipping product {ProductId} - no quantity change", change.ProductID);
                    continue;
                }

                // If QuantityChange is positive (customer increased order), we need to decrease stock
                // If QuantityChange is negative (customer decreased order), we need to increase stock
                int stockChange = -change.QuantityChange;
                
                _logger.LogInformation("Updating stock for product {ProductId} by {StockChange} (order quantity change was {QuantityChange})", 
                    change.ProductID, 
                    stockChange, 
                    change.QuantityChange);
                
                var result = await _productRepo.UpdateProductStock(change.ProductID, stockChange);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Successfully adjusted stock for product {ProductId} by {Adjustment}. New stock should be updated.", 
                        change.ProductID, 
                        stockChange);
                }
                else
                {
                    _logger.LogWarning("Failed to adjust stock for product {ProductId}: {Errors}", 
                        change.ProductID, 
                        string.Join(", ", result.Errors));
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

