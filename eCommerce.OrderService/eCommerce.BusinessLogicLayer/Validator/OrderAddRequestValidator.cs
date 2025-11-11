using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using eCommerce.BusinessLogicLayer.DTOs;

using FluentValidation;

namespace eCommerce.BusinessLogicLayer.Validator
{
    public class OrderAddRequestValidator : AbstractValidator<OrderAddRequest>
    {
        public OrderAddRequestValidator()
        {
            //UserID
            RuleFor(temp => temp.UserID)
              .NotEmpty().WithErrorCode("User ID can't be blank");

            //OrderDate
            RuleFor(temp => temp.OrderDate)
              .NotEmpty().WithErrorCode("Order Date can't be blank");

            //OrderItems
            RuleFor(temp => temp.OrderItems)
              .NotEmpty().WithErrorCode("Order Items can't be blank");
        }
    }
}
