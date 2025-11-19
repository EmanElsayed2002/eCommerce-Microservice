using System;
using System.Collections.Generic;

namespace eCommerce.BusinessLogicLayer.RabbitMQ
{
    public record OrderCreatedMessage(
        Guid OrderID,
        Guid UserID,
        decimal TotalBill,
        DateTime OrderDate,
        IEnumerable<OrderItemMessage> OrderItems);
}

