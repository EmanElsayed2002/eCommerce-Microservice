using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eCommerce.BusinessLogicLayer.DTOs
{
    public record ProductResponse(Guid ProductID, string ProductName, string Category, double? UnitPrice, int? QuantityInStock , string? ImagePath)
    {
        public ProductResponse() : this(default, default, default, default, default,default)
        {
        }
    }

}
