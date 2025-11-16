using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using eCommerce.BusinessLogicLayer.DTOs;

using FluentValidation;

namespace eCommerce.BusinessLogicLayer.Validator
{
    public class ProductUpdateRequestValidator : AbstractValidator<ProductUpdateRequest>
    {
        public ProductUpdateRequestValidator()
        {
            //ProductID
            RuleFor(temp => temp.ID)
              .NotEmpty().WithMessage("Product ID can't be blank");

            //ProductName
            RuleFor(temp => temp.Name)
              .NotEmpty().WithMessage("Product Name can't be blank");

            //Category
            RuleFor(temp => temp.Category)
              .IsInEnum().WithMessage("Category can't be blank");

            //UnitPrice
            RuleFor(temp => temp.Price)
              .InclusiveBetween(1, double.MaxValue).WithMessage($"Unit Price should between 1 to {double.MaxValue}");

            //QuantityInStock
            RuleFor(temp => temp.Quantity)
              .InclusiveBetween(1, int.MaxValue).WithMessage($"Quantity in Stock should between 1 to {int.MaxValue}");
        }
    }
}
