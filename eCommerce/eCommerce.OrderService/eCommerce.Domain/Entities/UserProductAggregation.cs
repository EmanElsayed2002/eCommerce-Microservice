using System;
using System.Collections.Generic;

namespace eCommerce.DataAccessLayer.Entities
{
    public class UserProductAggregation
    {
        public Guid UserID { get; set; }
        public List<UserProductAggregationProduct> Products { get; set; } = new();
    }

    public class UserProductAggregationProduct
    {
        public Guid ProductID { get; set; }
        public string ProductName { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalAmount { get; set; }
    }
}

