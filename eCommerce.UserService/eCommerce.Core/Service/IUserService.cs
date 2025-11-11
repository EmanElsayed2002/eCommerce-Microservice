using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using eCommerce.Core.DTOs;

using FluentResults;

namespace eCommerce.Core.Service
{
    public interface IUserService
    {
        Task<Result<AuthenticationResponse>> Login(LoginRequest request);
        Task<Result<AuthenticationResponse>> Register(RegisterRequest request);

        Task<Result<UserDTO>> GetUserById(Guid id);
    }
}
