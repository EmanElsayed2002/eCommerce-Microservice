using System;

namespace eCommerce.BusinessLogicLayer.RabbitMQ
{
    public record OrderItemMessage(Guid ProductID, int Quantity);
}

