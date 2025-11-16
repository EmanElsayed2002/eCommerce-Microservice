using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eCommerce.BusinessLogicLayer.DTOs
{
    public record ProductUpdateRequest(Guid ID, string Name, CategoryOptions Category, double Price, int? Quantity , string? ImagePath)
    {
        public ProductUpdateRequest() : this(default, default, default, default, default,default)
        {
        }
    }
}
