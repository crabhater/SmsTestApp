using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sms.Test.ConsoleApp.Services
{
    public class AppLogger
    {
        private readonly string _logFilePath;

        public AppLogger()
        {
            var date = DateTime.Now.ToString("yyyyMMdd");
            _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"test-sms-console-app-{date}.log");
        }

        public void WriteLine(string message = "")
        {
            Console.WriteLine(message);

            var fileLine = $"[{DateTime.Now:HH:mm:ss}] {message}";
            File.AppendAllText(_logFilePath, fileLine + Environment.NewLine, Encoding.UTF8);
        }

        public void WriteError(string message)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine($"ОШИБКА: {message}");

            Console.ForegroundColor = originalColor;

            var fileLine = $"[{DateTime.Now:HH:mm:ss}] ERROR: {message}";
            File.AppendAllText(_logFilePath, fileLine + Environment.NewLine, Encoding.UTF8);
        }
    }
}
