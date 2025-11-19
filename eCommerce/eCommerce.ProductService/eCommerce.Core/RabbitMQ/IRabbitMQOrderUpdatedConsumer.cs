namespace eCommerce.Core.RabbitMQ
{
    public interface IRabbitMQOrderUpdatedConsumer
    {
        Task Consume();
        void Dispose();
    }
}

