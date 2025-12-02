using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sms.Test.Network.Http.Models
{
    internal class BaseRequest<T>
    {
        public required string Command { get; set; }
        public required T CommandParameters { get; set; }
    }
}
