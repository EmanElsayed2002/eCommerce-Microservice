using System.Security.Policy;

using eCommerce.BusinessLogicLayer.DTOs;
using eCommerce.BusinessLogicLayer.Services;
using eCommerce.Shared.Resolver;

namespace eCommerce.API.Endpoints
{
    public static class ProductAPIS
    {
        public static IEndpointRouteBuilder MapProductAPIEndpoint(this IEndpointRouteBuilder app)
        {
         

            app.MapGet("/api/products/search/product-id/{productId:guid}", async (IProductService _service, HttpContext httpContext ,Guid productId) =>
            {
                var res = await _service.GetProductByCondition(x => x.ID == productId);
                return res.ResolveToIActionResult(successStatusCode: StatusCodes.Status200OK, context: httpContext, message: "Searching Successfully");
            });

            app.MapPut("/api/products", async (IProductService _service, HttpContext httpContext , ProductUpdateRequest request) =>
            {
                var res = await _service.UpdateProduct(request);
                return res.ResolveToIActionResult(successStatusCode: StatusCodes.Status200OK, context: httpContext, message: "updating Successfully");
            });

            app.MapPost("/api/products", async (IProductService _service, HttpContext httpContext , ProductAddRequest request) =>
            {
                var res = await _service.AddProduct(request);
                return res.ResolveToIActionResult(successStatusCode: StatusCodes.Status200OK, context: httpContext, message: "adding Successfully");
            });

            app.MapDelete("/api/products/{ProductId:guid}", async (IProductService _service, HttpContext httpContext ,Guid ProductId) =>
            {
                var res = await _service.DeleteProduct(ProductId);
                return res.ResolveToIActionResult(successStatusCode: StatusCodes.Status200OK, context: httpContext, message: "Deleting Successfully");
            });

            app.MapGet("/api/products", async (IProductService _service, HttpContext httpContext) =>
            {
                var res = await _service.GetAllProducts();
                return res.ResolveToIActionResult(successStatusCode: StatusCodes.Status200OK, context: httpContext, message: "Get All Product Successfully");
            });



            return app;
        }
        
    }
}
