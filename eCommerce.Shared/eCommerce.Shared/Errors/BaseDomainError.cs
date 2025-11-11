using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using eCommerce.Shared.DTOs;
using eCommerce.Shared.Enums;

using FluentResults;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

namespace eCommerce.Shared.Errors
{
    public abstract class BaseDomainError(string msg , ErrorCode error ,int statuscode) : Error(msg)
    {
        public ErrorCode Error { get; } = error;
        public int StatusCode { get;} = statuscode;
        public virtual IActionResult ToIActionResult(HttpContext context)
        {
            string endpointUrl = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
            var response = new FailureResponseDto(endpointUrl, Message, Error.ToString(), StatusCode, context.TraceIdentifier, []);
            return new JsonResult(response) { StatusCode = StatusCode};
        }
    }
}
