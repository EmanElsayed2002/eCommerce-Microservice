using System;
using System.Collections.Generic;

namespace eCommerce.BusinessLogicLayer.RabbitMQ
{
    public record OrderUpdatedMessage(
        Guid OrderID,
        Guid UserID,
        decimal TotalBill,
        DateTime OrderDate,
        IEnumerable<OrderItemDeltaMessage> ItemChanges);
}

