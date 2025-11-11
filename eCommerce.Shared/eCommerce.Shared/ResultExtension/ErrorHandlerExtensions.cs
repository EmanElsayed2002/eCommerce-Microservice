using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using eCommerce.Shared.DTOs;
using eCommerce.Shared.Enums;
using eCommerce.Shared.Errors;

using FluentResults;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.Shared.ResultExtension
{
    public static class ErrorHandlerExtensions
    {
        public static IActionResult HandleToIActionResult(this IError error, HttpContext context)
        {
            if (error is BaseDomainError resultError)
                return resultError.ToIActionResult(context);
            else
            {
                string endpointUrl = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";

                var failuresResponse = new FailureResponseDto(endpointUrl, error.Message, ErrorCode.BadRequest.ToString(), StatusCodes.Status400BadRequest, context.TraceIdentifier, []);

                // AddLog

                return new JsonResult(failuresResponse) { StatusCode = StatusCodes.Status400BadRequest };
            }
        }

        public static List<ValidationPropertyError> ToValidationPropertyErrors(this List<IError> errors)
        {
            return errors.OfType<ValidationError>().First().Errors;
        }
    }
}
