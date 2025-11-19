namespace eCommerce.BusinessLogicLayer.RabbitMQ;

public record ProductDeleteMessage(Guid ProductId, string ProductName);

