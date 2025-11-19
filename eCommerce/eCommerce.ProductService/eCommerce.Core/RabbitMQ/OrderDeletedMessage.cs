using System;
using System.Collections.Generic;

namespace eCommerce.BusinessLogicLayer.RabbitMQ
{
    public record OrderDeletedMessage(
        Guid OrderID,
        Guid UserID,
        IEnumerable<OrderItemMessage> OrderItems);
}

