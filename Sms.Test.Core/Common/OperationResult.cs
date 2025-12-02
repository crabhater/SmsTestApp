using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sms.Test.Core.Common
{
    public class OperationResult<T>
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public T? Data { get; set; }

        public static OperationResult<T> Success(T data) => new() { IsSuccess = true, Data = data };
        public static OperationResult<T> Failure(string message) => new() { IsSuccess = false, ErrorMessage = message };
    }
    public class OperationResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }

        public static OperationResult Success() => new() { IsSuccess = true };
        public static OperationResult Failure(string message) => new() { IsSuccess = false, ErrorMessage = message };
    }
}
