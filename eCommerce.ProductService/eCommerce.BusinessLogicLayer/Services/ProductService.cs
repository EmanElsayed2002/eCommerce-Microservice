using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using eCommerce.BusinessLogicLayer.DTOs;
using eCommerce.BusinessLogicLayer.Mapper;
using eCommerce.BusinessLogicLayer.RabbitMQ;
using eCommerce.DataAccessLayer.Models;
using eCommerce.DataAccessLayer.Repository;

using FluentResults;

using FluentValidation;

namespace eCommerce.BusinessLogicLayer.Services
{
    public class ProductService(IProductRepo _repo, IValidator<ProductAddRequest> addValidator, IValidator<ProductUpdateRequest> updateValidator, IRabbitMqPublisher _rabbitMqPublisher) : IProductService
    {
        public async Task<Result<Product>> AddProduct(ProductAddRequest product)
        {
            var _validator =await addValidator.ValidateAsync(product);
            if(!_validator.IsValid)
            {
                string erros = string.Join(",", _validator.Errors.Select(x => x.ErrorMessage));
                return Result.Fail(erros);
            }
            var res = await _repo.AddProduct(product.ToDo());
            if (res.IsFailed) return Result.Fail("Can not add Product");
            return Result.Ok(res.Value);
        }
        //
        public async Task<Result<bool>> DeleteProduct(Guid id)
        {
            var res = await _repo.GetProductByCondition(x => x.ID == id);
            if (res.IsFailed) return Result.Fail("this product not exist");
            var deleted = await _repo.DeleteProduct(id);
            if (deleted.IsSuccess)
            {
                ProductDeleteMessage productDeleteMessage = new ProductDeleteMessage(res.Value.ID, res.Value.Name);
              
                var headers = new Dictionary<string, object>()
                {
                    {"event", "product.delete" },
                    {"RowCount" , 1 }
                };

              await _rabbitMqPublisher.Publish(headers, productDeleteMessage);
                //_rabbitMqPublisher.push
                return Result.Ok();

            }
            return Result.Fail("Can not Delete this product");

        }

        public async Task<Result<IEnumerable<ProductResponse>>> GetAllProducts()
        {
            var res = await _repo.GetAll();
            if (res.IsSuccess) return Result.Ok(res.Value.ToDo());
            return Result.Fail("Not Exist");
        }

        public async Task<Result<ProductResponse>> GetProductByCondition(Expression<Func<Product, bool>> predictae)
        {
            var res = await _repo.GetProductByCondition(predictae);
            if (res.IsSuccess) return Result.Ok(res.Value.ToDo());
            return Result.Fail("Not Exist");
        }

        public async Task<Result<IEnumerable<ProductResponse>>> GetProductsByCondition(Expression<Func<Product, bool>> predictae)
        {
            var res = await _repo.GetProductsByCondition(predictae);
            if (res.IsSuccess) return Result.Ok(res.Value.ToDo());
            return Result.Fail("Not Exist");
        }

        public async Task<Result<Product>> UpdateProduct(ProductUpdateRequest product)
        {
            var _validator = await updateValidator.ValidateAsync(product);
            if (!_validator.IsValid) return Result.Fail("Not Validated");

            var productExisting = await _repo.GetProductByCondition(x => x.ID == product.ID);
            if(productExisting.IsFailed)
            {
                return Result.Fail("Not Exist");
            }
            var res = await _repo.UpdateProduct(product.ToDo());
            if (res.IsSuccess)
            {
                ProductUpdateMessage productDeleteMessage = new ProductUpdateMessage(res.Value.ID, product?.Name);
               
                var headers = new Dictionary<string, object>()
                {
                    {"event", "product.update" },
                    {"RowCount" , 1 }
                };
                await _rabbitMqPublisher.Publish(headers, productDeleteMessage);
                return Result.Ok(product.ToDo());
            }
            return Result.Fail("Can not Update the Product");
        }
    }
}
