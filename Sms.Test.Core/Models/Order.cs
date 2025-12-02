using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sms.Test.Core.Models
{
    public class Order
    {
        public Guid Id { get; set; }

        public List<OrderItem> Items { get; set; } = new();
    }
}
