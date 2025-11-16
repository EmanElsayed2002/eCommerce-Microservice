using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using eCommerce.DataAccessLayer.Models;
using eCommerce.Shared.DTOs;

using FluentResults;

namespace eCommerce.DataAccessLayer.Repository
{
    public interface IProductRepo
    {
        Task<Result<PagedList<Product>>> GetAll(int pageNumber, int pageSize);
        Task<Result<PagedList<Product>>> GetProductsByCondition(Expression<Func<Product, bool>> predicate, int pageNumber, int pageSize);
        Task<Result<Product>> GetProductByCondition(Expression<Func<Product, bool>> predicate);
        Task<Result<Product>> AddProduct(Product product);
        Task<Result<Product>> UpdateProduct(Product product);
        Task<Result<bool>> DeleteProduct(Guid id);
    }
}
