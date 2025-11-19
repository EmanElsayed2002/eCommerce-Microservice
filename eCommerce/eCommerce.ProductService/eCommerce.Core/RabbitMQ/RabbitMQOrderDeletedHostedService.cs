using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace eCommerce.Core.RabbitMQ
{
    public class RabbitMQOrderDeletedHostedService : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private IServiceScope? _scope;
        private IRabbitMQOrderDeletedConsumer? _orderDeletedConsumer;

        public RabbitMQOrderDeletedHostedService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _scope = _scopeFactory.CreateScope();
            _orderDeletedConsumer = _scope.ServiceProvider.GetRequiredService<IRabbitMQOrderDeletedConsumer>();
            _ = _orderDeletedConsumer.Consume();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _orderDeletedConsumer?.Dispose();
            _scope?.Dispose();
            return Task.CompletedTask;
        }
    }
}

