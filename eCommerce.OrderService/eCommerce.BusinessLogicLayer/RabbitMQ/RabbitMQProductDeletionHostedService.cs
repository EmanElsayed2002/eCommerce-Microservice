using Microsoft.Extensions.Hosting;

namespace eCommerce.BusinessLogicLayer.RabbitMQ;

public class RabbitMQProductDeletionHostedService : IHostedService
{
    private readonly IRabbitMQProductDeletionConsumer _productDeletionConsumer;

    public RabbitMQProductDeletionHostedService(IRabbitMQProductDeletionConsumer consumer)
    {
        _productDeletionConsumer = consumer;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _productDeletionConsumer.Consume();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _productDeletionConsumer.Dispose();
        return Task.CompletedTask;
    }
}

