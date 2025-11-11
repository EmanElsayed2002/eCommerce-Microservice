using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using eCommerce.Core.DTOs;

using FluentValidation;

namespace eCommerce.Core.Validator
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(temp => temp.Email)
        .NotEmpty().WithMessage("Email is required")
        .EmailAddress().WithMessage("Invalid email address format")
        ;

            //Password
            RuleFor(temp => temp.Password)
              .NotEmpty().WithMessage("Password is required")
              ;

            // Validate the PersonName property.
            RuleFor(request => request.PersonName)
                .NotEmpty().WithMessage("PersonName can't be blank")
                .Length(1, 50).WithMessage("Person Name should be 1 to 50 characters long");

            // Validate the Gender property.
            RuleFor(request => request.Gender)
                .IsInEnum().WithMessage("Invalid gender option");
        }
    }
    
}
