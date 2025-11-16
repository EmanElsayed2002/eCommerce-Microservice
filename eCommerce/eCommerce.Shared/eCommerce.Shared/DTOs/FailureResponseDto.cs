using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using eCommerce.Shared.Errors;

namespace eCommerce.Shared.DTOs
{
    public record FailureResponseDto
    (
        string Type,
        string Title,
        string ErrorCode,
        int StatusCode,
        string TraceId,
        List<ValidationPropertyError> Errors
    );
}
