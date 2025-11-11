using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using eCommerce.Core.DTOs;
using eCommerce.Core.Mapper;
using eCommerce.Core.Repository;

using FluentResults;

namespace eCommerce.Core.Service
{
    public class UserService(IUserRepo _repo) : IUserService
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
            var res = await _repo.GetUserByEmail(request.Email, request.Password);
            if(res.IsFailed)
            {
                return Result.Fail("user not exists");
            }
            
            
            return Result.Ok(res.Value.ToDo() with { Success = true , Token = "token"});
        }

        public async Task<Result<AuthenticationResponse>> Register(RegisterRequest request)
        {
            var user = request.ToDo();
            var res = await _repo.addUser(user);
            if(res.IsFailed) return Result.Fail(res.Errors);
            return Result.Ok( res.Value.ToDo());
        }
    }
}
