using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using eCommerce.Core.DTOs;
using eCommerce.Core.Models;

namespace eCommerce.Core.Mapper
{
    public static class UserProfile
    {
        public static User ToDo(this RegisterRequest request)
        {
            return new User
            {
                Email = request.Email,
                Name = request.PersonName,
                GenderOptions = request.Gender,
                Password = request.Password,
                UserId = Guid.NewGuid()
            };
        }

        public static AuthenticationResponse ToDo(this User request)
        {
            return new AuthenticationResponse
            {
                Email = request.Email,
                Gender = request.GenderOptions,
                PersonName = request.Name,
                UserID = request.UserId

            };
        }

        public static UserDTO TODO(this User request)
        {
            return new UserDTO(
                request.UserId,
                request.Email,
                request.Name,
                request.GenderOptions.ToString()
                );
        }
    }
}