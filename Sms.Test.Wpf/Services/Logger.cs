using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sms.Test.Wpf.Services
{
    public class Logger
    {
        private readonly string _logPath;

        public Logger()
        {
            // test-sms-wpf-app-yyyyMMdd.log
            _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"test-sms-wpf-app-{DateTime.Now:yyyyMMdd}.log");
        }

        public void LogChange(string variableName, string oldValue, string newValue)
        {
            var msg = $"[{DateTime.Now:HH:mm:ss}] Variable '{variableName}' changed. Old: '{oldValue}' -> New: '{newValue}'";
            try
            {
                File.AppendAllText(_logPath, msg + Environment.NewLine, Encoding.UTF8);
            }
            catch { /* Игнорируем ошибки логирования, чтобы не крашить UI */ }
        }
    }
}
