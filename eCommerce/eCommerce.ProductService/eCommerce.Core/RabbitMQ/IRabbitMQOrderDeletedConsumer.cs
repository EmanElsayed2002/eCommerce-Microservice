namespace eCommerce.Core.RabbitMQ;

public interface IRabbitMQOrderDeletedConsumer
{
    Task Consume();
    void Dispose();
}

