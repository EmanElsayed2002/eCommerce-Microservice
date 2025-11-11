using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using eCommerce.BusinessLogicLayer.DTOs;
using eCommerce.DataAccessLayer.Models;

using FluentResults;

namespace eCommerce.BusinessLogicLayer.Services
{
    public interface IProductService
    {
        Task<Result<Product>> AddProduct(ProductAddRequest product);
        Task<Result<Product>> UpdateProduct(ProductUpdateRequest product);
        Task<Result<bool>> DeleteProduct(Guid id);
        Task<Result<ProductResponse>> GetProductByCondition(Expression<Func<Product, bool>> predictae);
        Task<Result<IEnumerable< ProductResponse>>> GetProductsByCondition(Expression<Func<Product, bool>> predictae);
        Task<Result<IEnumerable<ProductResponse>>> GetAllProducts();

    }
}
