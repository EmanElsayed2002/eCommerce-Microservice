namespace eCommerce.Core.RabbitMQ;

public interface IRabbitMQOrderCreatedConsumer
{
    Task Consume();
    void Dispose();
}

