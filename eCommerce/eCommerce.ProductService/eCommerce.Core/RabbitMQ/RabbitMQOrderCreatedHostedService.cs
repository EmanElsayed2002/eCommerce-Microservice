using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace eCommerce.Core.RabbitMQ
{
    public class RabbitMQOrderCreatedHostedService : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private IServiceScope? _scope;
        private IRabbitMQOrderCreatedConsumer? _orderCreatedConsumer;

        public RabbitMQOrderCreatedHostedService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _scope = _scopeFactory.CreateScope();
            _orderCreatedConsumer = _scope.ServiceProvider.GetRequiredService<IRabbitMQOrderCreatedConsumer>();
            _ = _orderCreatedConsumer.Consume();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _orderCreatedConsumer?.Dispose();
            _scope?.Dispose();
            return Task.CompletedTask;
        }
    }
}

