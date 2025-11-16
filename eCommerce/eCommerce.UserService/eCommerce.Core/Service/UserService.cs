using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using eCommerce.Core.DTOs;
using eCommerce.Core.Mapper;
using eCommerce.Core.Repository;

using FluentResults;

using FluentValidation;

namespace eCommerce.Core.Service
{
    public class UserService(IUserRepo _repo, IValidator<RegisterRequest> RegisterValidator , IValidator<LoginRequest> LoginValidator) : IUserService
    {
        public async Task<Result<UserDTO>> GetUserById(Guid id)
        {
           var res = await _repo.GetUserById(id);
            if(res.IsFailed) return Result.Fail(res.Errors);
            var dto = res.Value.TODO();
            return Result.Ok(dto);
        }

        public async Task<Result<AuthenticationResponse>> Login(LoginRequest request)
        {
            var validator = await LoginValidator.ValidateAsync(request);
            if (!validator.IsValid)
            {
                return Result.Fail(validator.Errors.Select(x => x.ErrorMessage));
            }
            var res = await _repo.GetUserByEmail(request.Email, request.Password);
            if(res.IsFailed)
            {
                return Result.Fail("user not exists");
            }
            
            
            return Result.Ok(res.Value.ToDo() with { Success = true , Token = "token"});
        }

        public async Task<Result<AuthenticationResponse>> Register(RegisterRequest request)
        {
            var validator = await RegisterValidator.ValidateAsync(request);
            if (!validator.IsValid)
            {
                return Result.Fail(validator.Errors.Select(x => x.ErrorMessage));
            }
            var user = request.ToDo();
            var res = await _repo.addUser(user);
            if(res.IsFailed) return Result.Fail(res.Errors);
            return Result.Ok( res.Value.ToDo());
        }
    }
}
