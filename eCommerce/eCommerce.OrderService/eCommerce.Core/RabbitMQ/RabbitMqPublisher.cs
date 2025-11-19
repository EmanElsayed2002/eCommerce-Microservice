using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using RabbitMQ.Client;

namespace eCommerce.BusinessLogicLayer.RabbitMQ
{
    public class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
    {
        public readonly ConnectionFactory factory;
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly IConfiguration configuration;

        public RabbitMqPublisher(IConfiguration _configuration)
        {
            configuration = _configuration;
            
            // Read from environment variables first, then fall back to configuration
            string hostName = Environment.GetEnvironmentVariable("RabbitMQ_HostName") 
                ?? _configuration["RabbitMQ_HostName"] 
                ?? throw new ArgumentNullException(nameof(_configuration), "RabbitMQ_HostName configuration is missing. Set RabbitMQ_HostName environment variable or in appsettings.json");
            
            // Validate hostname is not empty or whitespace
            if (string.IsNullOrWhiteSpace(hostName))
            {
                throw new ArgumentException("RabbitMQ_HostName cannot be empty or whitespace. Set RabbitMQ_HostName environment variable or in appsettings.json", nameof(_configuration));
            }
            
            string userName = Environment.GetEnvironmentVariable("RabbitMQ_UserName") 
                ?? _configuration["RabbitMQ_UserName"] 
                ?? throw new ArgumentNullException(nameof(_configuration), "RabbitMQ_UserName configuration is missing. Set RabbitMQ_UserName environment variable or in appsettings.json");
            
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentException("RabbitMQ_UserName cannot be empty or whitespace. Set RabbitMQ_UserName environment variable or in appsettings.json", nameof(_configuration));
            }
            
            string password = Environment.GetEnvironmentVariable("RabbitMQ_Password") 
                ?? _configuration["RabbitMQ_Password"] 
                ?? throw new ArgumentNullException(nameof(_configuration), "RabbitMQ_Password configuration is missing. Set RabbitMQ_Password environment variable or in appsettings.json");
            
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("RabbitMQ_Password cannot be empty or whitespace. Set RabbitMQ_Password environment variable or in appsettings.json", nameof(_configuration));
            }
            
            string port = Environment.GetEnvironmentVariable("RabbitMQ_Port") 
                ?? _configuration["RabbitMQ_Port"] 
                ?? throw new ArgumentNullException(nameof(_configuration), "RabbitMQ_Port configuration is missing. Set RabbitMQ_Port environment variable or in appsettings.json");
            
            if (!int.TryParse(port, out int portNumber))
            {
                throw new ArgumentException($"Invalid RabbitMQ_Port value: '{port}'. Port must be a valid integer.", nameof(_configuration));
            }

            factory = new ConnectionFactory()
            {
                HostName = hostName.Trim(),
                UserName = userName.Trim(),
                Password = password.Trim(),
                Port = portNumber
            };

            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
        }
        
        public async Task Publish<T>(Dictionary<string, object> headers, T message)
        {
            string messageJson = JsonSerializer.Serialize(message);
            byte[] messageBodyInByes = Encoding.UTF8.GetBytes(messageJson);

            string exchange =
                Environment.GetEnvironmentVariable("RabbitMQ_Orders_Exchange")
                ?? Environment.GetEnvironmentVariable("RabbitMQ_Exchange")
                ?? configuration["RabbitMQ_Orders_Exchange"]
                ?? configuration["RabbitMQ_Exchange"]
                ?? "order.exchange";

            await _channel.ExchangeDeclareAsync(
                exchange: exchange,
                type: ExchangeType.Headers,
                durable: true,
                autoDelete: false,
                arguments: null);

            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                Headers = headers ?? new Dictionary<string, object>()
            };

            await _channel.BasicPublishAsync(
                exchange: exchange,
                routingKey: string.Empty,
                mandatory: false,
                basicProperties: properties,
                body: messageBodyInByes);
        }
        
        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}

