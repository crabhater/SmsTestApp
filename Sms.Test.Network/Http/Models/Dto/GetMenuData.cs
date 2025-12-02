using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sms.Test.Network.Http.Models.Dto
{
    internal class GetMenuData
    {
        public List<MenuItemDto> MenuItems { get; set; } = new();
    }
}
