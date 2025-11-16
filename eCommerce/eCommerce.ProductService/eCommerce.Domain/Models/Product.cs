using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eCommerce.DataAccessLayer.Models
{
    public class Product
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public double Price { get; set; }
        public int? Quantity { get; set; }
        public string? ImagePath { get; set; }

    }
}
