using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using eCommerce.Shared.DTOs;
using eCommerce.Shared.Enums;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.Shared.Errors
{
    public class ValidationError(List<ValidationPropertyError> errors) : BaseDomainError("Invalid Body Input", ErrorCode.InvalidBodyInput, StatusCodes.Status400BadRequest)
    {
        public List<ValidationPropertyError> Errors { get; init; } = errors;

        public override IActionResult ToIActionResult(HttpContext context)
        {
            string endpointUrl = "https://tools.ietf.org/html/rfc9110#section-15.5.1";

            var failuresResponse = new FailureResponseDto(endpointUrl, Message, ErrorCode.InvalidBodyInput.ToString(), StatusCode, context.TraceIdentifier, Errors);

            return new JsonResult(failuresResponse) { StatusCode = StatusCode };
        }
    }
}
