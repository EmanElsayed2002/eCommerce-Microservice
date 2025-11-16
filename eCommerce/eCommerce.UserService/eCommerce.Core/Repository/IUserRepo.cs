using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using eCommerce.Core.Models;

using FluentResults;

namespace eCommerce.Core.Repository
{
    public interface IUserRepo
    {
        Task<Result<User>> addUser(User user);

        Task<Result<User>> GetUserById(Guid id);

        Task<Result<User>> GetUserByEmail(string email , string password);
    }
}
