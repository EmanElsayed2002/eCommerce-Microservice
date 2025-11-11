using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using eCommerce.Core.Enum;

namespace eCommerce.Core.DTOs
{
    public record AuthenticationResponse(
      Guid UserID,
      string? Email,
      string? PersonName,
      GenderOptions? Gender,
      string? Token,
      bool Success
    )
    {
    
        public AuthenticationResponse() : this(default, default, default, default, default, default)
        {
        }
    }
  
}
