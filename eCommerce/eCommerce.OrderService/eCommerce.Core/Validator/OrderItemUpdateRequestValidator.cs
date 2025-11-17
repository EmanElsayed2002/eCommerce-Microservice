using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using eCommerce.BusinessLogicLayer.DTOs;

using FluentValidation;

namespace eCommerce.BusinessLogicLayer.Validator
{

    public class OrderItemUpdateRequestValidator : AbstractValidator<OrderItemUpdateRequest>
    {
        public OrderItemUpdateRequestValidator()
        {
            //ProductID
            RuleFor(temp => temp.ProductID)
              .NotEmpty().WithErrorCode("Product ID can't be blank");

        
            //Quantity
            RuleFor(temp => temp.Quantity)
              .NotEmpty().WithErrorCode("Quantity can't be blank")
              .GreaterThan(0).WithErrorCode("Quantity can't be less than or equal to zero");
        }
    }

}
