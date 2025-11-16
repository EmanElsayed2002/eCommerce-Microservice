using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dapper;

using eCommerce.Core.Models;
using eCommerce.Core.Repository;
using eCommerce.Infrastructure.DbContext;

using FluentResults;

namespace eCommerce.Infrastructure.Repository
{
    public class UserRepo(AppDbContext _context) : IUserRepo
    {
       
        public async Task<Result<User>> addUser(User user)
        {
            var query = "SELECT * FROM public.\"Users\" WHERE \"Email\"=@Email";
            var _user = await _context.dbConnection.QueryFirstOrDefaultAsync<User>(query, new { Email = user.Email });
            if (_user != null)
            {
                return Result.Fail("Email Already Exist");
            }
            user.UserId = Guid.NewGuid();
            query = "INSERT INTO public.\"Users\"(\"UserId\", \"Email\", \"Name\", \"Gender\", \"Password\") VALUES(@UserID, @Email, @PersonName, @Gender, @Password)";
            var parameters = new
            {
                UserID = user.UserId,
                Email = user.Email,
                PersonName = user.Name,
                Gender = user.GenderOptions,
                Password = user.Password
            };
            int rowAffected = await _context.dbConnection.ExecuteAsync(query, parameters);
            if(rowAffected > 0) return Result.Ok(user);
            return Result.Fail("Failed to add user");
        }

        public async Task<Result<User>> GetUserByEmail(string email , string password)
        {
            var query = "SELECT * FROM public.\"Users\" WHERE \"Email\"=@Email AND \"Password\"=@Password";
            var parameters = new { Email = email, Password = password };
            var user = await _context.dbConnection.QueryFirstOrDefaultAsync<User>(query, parameters);
            if (user == null) return Result.Fail("User Not Exist"); 
            return Result.Ok(user);
        }

        public async Task<Result<User>> GetUserById(Guid id)
        {
            var query = "SELECT * FROM public.\"Users\" WHERE \"UserId\" = @UserID";
            var user = await _context.dbConnection.QueryFirstOrDefaultAsync<User>(query, new { UserId = id });
            if (user == null) return Result.Fail("Not Found This User");
            return Result.Ok(user);
        }
    }
}
