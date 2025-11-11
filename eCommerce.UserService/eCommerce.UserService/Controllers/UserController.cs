using eCommerce.Core.DTOs;
using eCommerce.Core.Service;
using eCommerce.Shared.Resolver;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IUserService _service) : ControllerBase
    {
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await _service.Login(request);
            return result.ResolveToIActionResult(successStatusCode: StatusCodes.Status200OK, context: HttpContext, message: "Login Request Successfully");
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var result = await _service.Register(request);
            return result.ResolveToIActionResult(successStatusCode: StatusCodes.Status200OK, context: HttpContext, message: "Register Request Successfully");
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var result = await _service.GetUserById(id);
            return result.ResolveToIActionResult(successStatusCode: StatusCodes.Status200OK, context: HttpContext, message: "Getting User Request Successfully");

        }
    }
}
