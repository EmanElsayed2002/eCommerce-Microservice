using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using eCommerce.BusinessLogicLayer.DTOs;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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

        public RabbitMQProductNameUpdateConsumer(
            IConfiguration configuration, 
            ILogger<RabbitMQProductNameUpdateConsumer> logger, 
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
                byte[] body = args.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);

                if (message != null)
                {
                    ProductDTO? productDTO = JsonSerializer.Deserialize<ProductDTO>(message);

                    if (productDTO != null)
                    {
                        await HandleProductUpdation(productDTO);
                    }
                }
            };

           await _channel.BasicConsumeAsync(queue: queueName, consumer: consumer, autoAck: true);
        }

        private async Task HandleProductUpdation(ProductDTO productDTO)
        {
            _logger.LogInformation($"Product name updated: {productDTO.ProductID}, New name: {productDTO.ProductName}");

            // Update cache with new product information
            // Note: Cache key format matches ProductsMicroserviceClient: "product: {productID}"
            string productJson = JsonSerializer.Serialize(productDTO);
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(300));

            string cacheKeyToWrite = $"product: {productDTO.ProductID}";
            await _cache.SetStringAsync(cacheKeyToWrite, productJson, options);
            _logger.LogInformation($"Updated product {productDTO.ProductID} in cache");
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}

