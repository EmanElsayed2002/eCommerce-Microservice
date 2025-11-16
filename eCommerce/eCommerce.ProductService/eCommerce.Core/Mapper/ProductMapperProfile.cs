using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using eCommerce.BusinessLogicLayer.DTOs;
using eCommerce.DataAccessLayer.Models;

using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.BusinessLogicLayer.Mapper
{
    public static class ProductMapperProfile
    {
        public static Product ToDo(this ProductAddRequest request)
        {
            return new Product
            {
                ID = Guid.NewGuid(),
                Name = request.Name,
                Category = request.Category.ToString(),
                Price = request.Price,
                Quantity = request.Quantity,

            };

        } 
        public static ProductResponse ToDo(this Product request)
        {
            return new ProductResponse
            {
                Category = request.Category,
                ProductID = request.ID,
                ProductName = request.Name,
                QuantityInStock = request.Quantity,
                UnitPrice = request.Price
            };
        }

        public static IEnumerable<ProductResponse> ToDo(this IEnumerable<Product> products)
        {
            return products.Select(p => p.ToDo());
        }

        public static Product ToDo(this ProductUpdateRequest request)
        {
            return new Product
            {
                Category = request.Category.ToString(),
                ID = request.ID,
                Name = request.Name,
                Price = request.Price,
                Quantity = request.Quantity

            };
        }
    }
}
