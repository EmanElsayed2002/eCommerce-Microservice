using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eCommerce.Shared.Enums
{
    public enum ErrorCode
    {
        BadRequest,
        InvalidBodyInput,
        EntityNotFound,
        LookUpNotFound,
        EntityAlreadyExists,
        InternalServerError,
        InvalidFileExtension,
        IncorrectFileType,
        SmsSendingError,
        FileSizeExceeded,
        InvalidFile
    }
}
