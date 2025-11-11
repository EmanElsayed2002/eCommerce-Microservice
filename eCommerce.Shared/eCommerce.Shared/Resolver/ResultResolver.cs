using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using eCommerce.Shared.DTOs;
using eCommerce.Shared.ResultExtension;

using FluentResults;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.Shared.Resolver
{
    public static class ResultResolver
    {
        /*
    * Sucess => Return Specified Action Result with Data
    * Failed => Return Error
    * Example => ResolveToIActionResult(x => Ok, HttpContext)
    */
        public static IActionResult ResolveToIActionResult<T>(this Result<T> result, Func<T, IActionResult> successAction, HttpContext context) =>
            result.IsSuccess ? successAction(result.Value) : result.Errors[0].HandleToIActionResult(context);

        /*
         * Sucess => Return SuccessResponseDto with No Data
         * Failed => Return Error
         * Example => ResolveToIActionResult(StatusCodes.Status200OK, HttpContext)
         */
        public static IActionResult ResolveToIActionResult(this Result result, int successStatusCode, HttpContext context, string message = "Action has been done successfully") =>
            result.IsSuccess ? new JsonResult(new SuccessResponseDto<object>(null, message)) { StatusCode = successStatusCode } : result.Errors[0].HandleToIActionResult(context);

        /*
         * Sucess => Return SuccessResponseDto with Mapped Data
         * Failed => Return Error
         * Example => ResolveToIActionResult(x => x.Id, StatusCodes.Status200OK, HttpContext)
         */
        public static IActionResult ResolveToIActionResult<Tin, Tout>
            (this Result<Tin> result, Func<Tin, Tout> successAction, int successStatusCode, HttpContext context, string message = "Action has been done successfully") =>
                result.IsSuccess ? new JsonResult(new SuccessResponseDto<Tout>(successAction(result.Value), message)) { StatusCode = successStatusCode } : result.Errors[0].HandleToIActionResult(context);

        /*
         * Sucess => Return SuccessResponseDto with Original Data
         * Failed => Return Error
         * Example => ResolveToIActionResult(StatusCodes.Status200OK, HttpContext)
         */
        public static IActionResult ResolveToIActionResult<Tin>(this Result<Tin> result, int successStatusCode, HttpContext context, string message = "Action has been done successfully") =>
            ResolveToIActionResult(result, x => x, successStatusCode, context, message);
    }
}
