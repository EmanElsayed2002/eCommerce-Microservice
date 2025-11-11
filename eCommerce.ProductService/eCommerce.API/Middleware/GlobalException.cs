using Microsoft.AspNetCore.Http;

using System.Text.Json;

namespace eCommerce.API.Middleware
{
    public class GlobalMiddleware(RequestDelegate _next, ILogger<GlobalMiddleware> _logger, IWebHostEnvironment _env)
    {

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the request: {Message}", ex.Message);

                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";

                var errorResponse = new
                {
                    error = "An internal server error occurred.",
                    message = ex.Message,
                    details = _env.IsDevelopment() ? ex.ToString() : null,
                    stackTrace = _env.IsDevelopment() ? ex.StackTrace : null
                };

                var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                await context.Response.WriteAsync(jsonResponse);
            }
        }
    }

    public static class GlobalMiddlewareExtensions
    {
        public static IApplicationBuilder UserGlobalException(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalMiddleware>();
        }
    }
}
