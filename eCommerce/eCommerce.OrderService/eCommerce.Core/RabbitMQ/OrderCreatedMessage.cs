using System;

namespace eCommerce.BusinessLogicLayer.RabbitMQ
{
    public record OrderCreatedMessage(Guid OrderID, Guid UserID, decimal TotalBill, DateTime OrderDate);
}

