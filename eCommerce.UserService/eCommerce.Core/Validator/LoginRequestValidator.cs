using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using eCommerce.Core.DTOs;

using FluentValidation;

namespace eCommerce.Core.Validator
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            //Email
            RuleFor(temp => temp.Email)
              .NotEmpty().WithMessage("Email is required")
              .EmailAddress().WithMessage("Invalid email address format")
              ;

            //Password
            RuleFor(temp => temp.Password)
              .NotEmpty().WithMessage("Password is required")
              ;
        }
    }
}
