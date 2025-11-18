using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eCommerce.BusinessLogicLayer.DTOs
{

    public record ProductDTO(Guid ProductID, string? ProductName, string? Category, decimal UnitPrice, int QuantityInStock , string? ImagePath);

}
