using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace eCommerce.BusinessLogicLayer.RabbitMQ;

public class RabbitMQProductNameUpdateHostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private IServiceScope? _scope;
    private IRabbitMQProductNameUpdateConsumer _consumer;
    public RabbitMQProductNameUpdateHostedService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _scope = _scopeFactory.CreateScope();
        _consumer = _scope.ServiceProvider.GetRequiredService<IRabbitMQProductNameUpdateConsumer>();
        _ = _consumer.Consume();
        return Task.CompletedTask;

    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _consumer?.Dispose();
        _scope?.Dispose();
        return Task.CompletedTask;
    }
}

