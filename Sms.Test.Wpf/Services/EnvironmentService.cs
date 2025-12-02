using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sms.Test.Wpf.Services
{
    public class EnvironmentService
    {
        private const EnvironmentVariableTarget Target = EnvironmentVariableTarget.User;

        public string GetVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name, Target) ?? string.Empty;
        }

        public void SetVariable(string name, string value)
        {

            if (string.IsNullOrEmpty(value))
            {
                Environment.SetEnvironmentVariable(name, null, Target);
            }
            else
            {
                Environment.SetEnvironmentVariable(name, value, Target);
            }
        }
    }
}
