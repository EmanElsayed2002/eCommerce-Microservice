using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using eCommerce.DataAccessLayer.Models;
using eCommerce.DataAccessLayer.MyDbContext;
using eCommerce.Shared.DTOs;

using FluentResults;

using Microsoft.EntityFrameworkCore;

namespace eCommerce.DataAccessLayer.Repository
{
    public class ProductRepo : IProductRepo
    {
        private readonly AppDbContext _context;

        public ProductRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<Product>> AddProduct(Product product)
        {
            try
            {
                await _context.Products.AddAsync(product);
                await _context.SaveChangesAsync();
                return Result.Ok(product);
            }
            catch (Exception ex)
            {
                return Result.Fail<Product>(ex.Message);
            }
        }

        public async Task<Result<bool>> DeleteProduct(Guid id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.ID == id);
            if (product == null)
            {
                return Result.Fail<bool>("Product not found");
            }

            try
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                return Result.Ok(true);
            }
            catch (Exception ex)
            {
                return Result.Fail<bool>(ex.Message);
            }
        }

        public async Task<Result<PagedList<Product>>> GetAll(int pageNumber, int pageSize)
        {
            var count = await _context.Products.CountAsync();
            var products = await _context.Products.OrderBy(x=>x.ID).Skip(pageSize * (pageNumber -1)).Take(pageSize).ToListAsync();

            return Result.Ok(new PagedList<Product>(products , count , pageNumber , pageSize));
        }

      

        public async Task<Result<Product>> GetProductByCondition(Expression<Func<Product, bool>> predicate)
        {
            var product = await _context.Products.FirstOrDefaultAsync(predicate);
            return product is null
                ? Result.Fail<Product>("Product not found")
                : Result.Ok(product);
        }

        public async Task<Result<PagedList<Product>>> GetProductsByCondition(Expression<Func<Product, bool>> predicate, int pageNumber, int pageSize)
        {
            var count = await _context.Products.Where(predicate).CountAsync();
            var products = await _context.Products.Where(predicate).OrderBy(x => x.ID).Skip(pageSize * (pageNumber - 1)).Take(pageSize).ToListAsync();

            return Result.Ok(new PagedList<Product>(products, count, pageNumber, pageSize));
           
        }

       
        public async Task<Result<Product>> UpdateProduct(Product product)
        {
            var existing = await _context.Products.FindAsync(product.ID);
            if (existing == null)
                return Result.Fail<Product>("Product not found");

            try
            {
                _context.Entry(existing).CurrentValues.SetValues(product);
                await _context.SaveChangesAsync();
                return Result.Ok(product);
            }
            catch (Exception ex)
            {
                return Result.Fail<Product>(ex.Message);
            }
        }
        public async Task<Result<bool>> UpdateProductStock(Guid productId, int quantityChange)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.ID == productId);
            if (product == null)
            {
                return Result.Fail<bool>("Product not found");
            }

            try
            {
                // If Quantity is null, treat it as 0
                int currentStock = product.Quantity ?? 0;
                int newStock = currentStock + quantityChange;

                // Prevent negative stock
                if (newStock < 0)
                {
                    return Result.Fail<bool>($"Insufficient stock. Available: {currentStock}, Requested: {Math.Abs(quantityChange)}");
                }

                product.Quantity = newStock;
                await _context.SaveChangesAsync();
                return Result.Ok(true);
            }
            catch (Exception ex)
            {
                return Result.Fail<bool>(ex.Message);
            }
        }
    }
}
