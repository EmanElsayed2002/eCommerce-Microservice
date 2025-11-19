using System;

namespace eCommerce.BusinessLogicLayer.RabbitMQ
{
    public record OrderItemDeltaMessage(Guid ProductID, int QuantityChange);
}

