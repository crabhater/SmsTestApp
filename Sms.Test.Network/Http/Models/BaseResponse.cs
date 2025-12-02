using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sms.Test.Network.Http.Models
{
    internal class BaseResponse<T>
    {
        public required string Command { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public T? Data { get; set; }
    }
}
