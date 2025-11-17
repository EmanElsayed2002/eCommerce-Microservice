using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eCommerce.BusinessLogicLayer.DTOs
{
    public record OrderItemAddRequest(Guid ProductID, int Quantity)
    {
        public OrderItemAddRequest() : this(default, default)
        {
        }
    }
}
