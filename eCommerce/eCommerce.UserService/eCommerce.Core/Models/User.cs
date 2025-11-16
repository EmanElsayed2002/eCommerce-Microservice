using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using eCommerce.Core.Enum;

namespace eCommerce.Core.Models
{
    public class User
    {
        public Guid UserId { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Name { get; set; }
        public GenderOptions GenderOptions { get; set; }
    }
}
