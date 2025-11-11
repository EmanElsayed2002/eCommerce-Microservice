using System;

namespace eCommerce.BusinessLogicLayer.RabbitMQ
{
    public record OrderUpdatedMessage(Guid OrderID, Guid UserID, decimal TotalBill, DateTime OrderDate);
}

