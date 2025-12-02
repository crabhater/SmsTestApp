using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sms.Test.Core.Models
{
    public class MenuItem
    {
        public required string Id { get; set; }

        public required string Article { get; set; }

        public required string Name { get; set; }

        public decimal Price { get; set; }

        public bool IsWeighted { get; set; }

        public required string FullPath { get; set; }

        public List<string> Barcodes { get; set; } = new();

        public override string ToString()
        {
            return $"{Name} ({Article}) - {Price} руб.";
        }
    }

}
