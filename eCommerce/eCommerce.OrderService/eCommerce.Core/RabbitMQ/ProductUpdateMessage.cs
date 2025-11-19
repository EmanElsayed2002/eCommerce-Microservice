using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eCommerce.BusinessLogicLayer.RabbitMQ
{
    public record ProductUpdateMessage
    (Guid ProductId , string? NewName);
}
