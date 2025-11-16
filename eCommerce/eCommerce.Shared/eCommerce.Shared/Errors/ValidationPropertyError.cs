using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentValidation.Results;

namespace eCommerce.Shared.Errors
{
    public record ValidationPropertyError(string PropertyName , List<string> Messages)
    {
        public ValidationPropertyError(ValidationFailure validationFailure) : this(validationFailure.PropertyName, [validationFailure.ErrorMessage])
        {
        }
    }
}
