using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eCommerce.BusinessLogicLayer.DTOs
{
    public record ProductAddRequest(string Name, CategoryOptions Category, double Price, int? Quantity , string? ImagePath)
    {
        public ProductAddRequest() : this(default, default, default, default,default)
        {
        }
    }

}
