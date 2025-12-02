using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sms.Test.Network.Http.Models.Dto
{
    internal class SendOrderParams
    {
        public string OrderId { get; set; } = "";
        public List<OrderItemDto> MenuItems { get; set; } = new();
    }
}
