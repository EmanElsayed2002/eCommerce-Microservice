using System;

namespace eCommerce.BusinessLogicLayer.RabbitMQ
{
    public record OrderDeletedMessage(Guid OrderID, Guid UserID);
}

