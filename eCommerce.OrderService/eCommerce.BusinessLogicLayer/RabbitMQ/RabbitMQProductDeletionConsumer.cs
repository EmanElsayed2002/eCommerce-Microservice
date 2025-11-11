using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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

        public RabbitMQProductDeletionConsumer(
            IConfiguration configuration, 
            ILogger<RabbitMQProductDeletionConsumer> logger, 
            IDistributedCache cache)
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
                byte[] body = args.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);

                if (message != null)
                {
                    ProductDeletionMessage? productDeletionMessage = JsonSerializer.Deserialize<ProductDeletionMessage>(message);

                    if (productDeletionMessage != null)
                    {
                        _logger.LogInformation($"Product deleted: {productDeletionMessage.ProductID}, Product name: {productDeletionMessage.ProductName}");
                        await HandleProductDeletion(productDeletionMessage.ProductID);
                    }
                }
            };

          await  _channel.BasicConsumeAsync(queue: queueName, consumer: consumer, autoAck: true);
        }

        private async Task HandleProductDeletion(Guid productID)
        {
            // Remove product from cache when it's deleted
            // Note: Cache key format matches ProductsMicroserviceClient: "product: {productID}"
            string cacheKeyToWrite = $"product: {productID}";
            await _cache.RemoveAsync(cacheKeyToWrite);
            _logger.LogInformation($"Removed product {productID} from cache");
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}

