using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eCommerce.Shared.DTOs
{
    public record SuccessResponseDto<T>
    (T? Data, string Message);
}
