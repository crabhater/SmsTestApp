using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Sms.Test.Wpf.Models;
using Sms.Test.Wpf.Services;
using System.Windows.Input;
using System.Windows;

namespace Sms.Test.Wpf.ViewModels
{
    public class MainViewModel
    {
        private readonly EnvironmentService _envService;
        private readonly Logger _logger;

        public ObservableCollection<EnvVariable> Variables { get; set; } = new();

        public ICommand SaveCommand { get; }
        public ICommand ReloadCommand { get; }

        public MainViewModel()
        {
            _envService = new EnvironmentService();
            _logger = new Logger();

            SaveCommand = new RelayCommand(_ => SaveChanges());
            ReloadCommand = new RelayCommand(_ => LoadVariables());

            LoadVariables();
        }

        private void LoadVariables()
        {
            Variables.Clear();
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true);

            var config = builder.Build();
            var names = config.GetSection("TargetVariables")
                              .GetChildren()
                              .Select(x => x.Value!) 
                              .Where(x => x != null)
                              .ToArray();

            foreach (var name in names)
            {
                var currentVal = _envService.GetVariable(name);
                Variables.Add(new EnvVariable
                {
                    Name = name,
                    Value = currentVal,
                    OriginalValue = currentVal 
                });
            }
        }

        private void SaveChanges()
        {
            int changedCount = 0;

            foreach (var item in Variables)
            {
                if (item.Value != item.OriginalValue)
                {
                    try
                    {
                        _envService.SetVariable(item.Name, item.Value);
                        _logger.LogChange(item.Name, item.OriginalValue, item.Value);

                        item.OriginalValue = item.Value;
                        changedCount++;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка сохранения {item.Name}: {ex.Message}");
                    }
                }
            }

            if (changedCount > 0)
            {
                MessageBox.Show($"Успешно обновлено переменных: {changedCount}", "Сохранение");
            }
            else
            {
                MessageBox.Show("Нет изменений для сохранения.", "Сохранение");
            }
        }
    }
}
