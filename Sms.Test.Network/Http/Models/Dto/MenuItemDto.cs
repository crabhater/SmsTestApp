using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sms.Test.Network.Http.Models.Dto
{
    internal class MenuItemDto
    {
        public string Id { get; set; } = "";
        public string Article { get; set; } = "";
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public bool IsWeighted { get; set; }
        public string FullPath { get; set; } = "";
        public List<string> Barcodes { get; set; } = new();
    }
}
