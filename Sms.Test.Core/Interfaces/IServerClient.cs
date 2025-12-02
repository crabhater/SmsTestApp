using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sms.Test.Core.Common;
using Sms.Test.Core.Models;

namespace Sms.Test.Core.Interfaces
{
    public interface IServerClient
    {
        Task<OperationResult<List<MenuItem>>> GetMenuAsync(CancellationToken cancellationToken = default);

        Task<OperationResult> SendOrderAsync(Order order, CancellationToken cancellationToken = default);
    }
}
