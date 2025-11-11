namespace eCommerce.BusinessLogicLayer.RabbitMQ;

public interface IRabbitMQProductDeletionConsumer
{
    Task Consume();
    void Dispose();
}

