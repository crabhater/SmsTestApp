using Microsoft.Extensions.Configuration;
using Sms.Test.ConsoleApp.Data;
using Sms.Test.ConsoleApp.Services;
using Sms.Test.Core.Interfaces;
using Sms.Test.Network.Grpc;
using Sms.Test.Network;
using System.Globalization;
using Sms.Test.Core.Models;

namespace Sms.Test.ConsoleApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // 1. Настройка конфигурации
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);

            IConfiguration config = builder.Build();

            // 2. Инициализация сервисов
            var logger = new AppLogger();
            logger.WriteLine("=== Запуск приложения ===");

            var connectionString = config["Database:ConnectionString"];
            if (string.IsNullOrEmpty(connectionString))
            {
                logger.WriteError("Не указана строка подключения к БД.");
                return;
            }

            var dbService = new DatabaseService(connectionString, logger);

            // 3. Выбор клиента (HTTP или gRPC)
            IServerClient client;
            string protocol = config["Api:Protocol"] ?? "Http";
            string baseUrl = config["Api:BaseUrl"] ?? "http://localhost:5000";

            if (protocol.Equals("Grpc", StringComparison.OrdinalIgnoreCase))
            {
                logger.WriteLine($"Используется протокол: gRPC ({baseUrl})");
                client = new GrpcServerClient(baseUrl);
            }
            else
            {
                logger.WriteLine($"Используется протокол: HTTP ({baseUrl})");
                var httpClient = new HttpClient();
                client = new HttpServerClient(httpClient, baseUrl, config["Api:Username"]!, config["Api:Password"]!);
            }

            // === Инициализация БД ===
            try
            {
                await dbService.InitializeAsync();
            }
            catch
            {
                return; 
            }

            // === Получение меню ===
            logger.WriteLine("Запрос меню с сервера...");
            var menuResult = await client.GetMenuAsync();

            if (!menuResult.IsSuccess || menuResult.Data == null)
            {
                logger.WriteError($"Ошибка получения меню: {menuResult.ErrorMessage}");
                return;
            }

            var menuItems = menuResult.Data;

            // === Запись в БД и Вывод ===
            await dbService.SaveMenuItemsAsync(menuItems);

            logger.WriteLine("\n--- МЕНЮ ---");
            foreach (var item in menuItems)
            {
                logger.WriteLine($"{item.Name} – {item.Article} – {item.Price:F2}");
            }
            logger.WriteLine("------------\n");


            // === ШАГ 4, 5, 6: Ввод заказа ===
            var order = new Order
            {
                Id = Guid.NewGuid()
            };

            while (true)
            {
                logger.WriteLine("Введите заказ в формате: Код1:Количество1;Код2:Количество2");
                logger.WriteLine("Пример: A1004292:1;A1004293:0.5");
                Console.Write("> ");

                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) continue;

                logger.WriteLine($"Ввод пользователя: {input}");

                var parsedItems = new List<OrderItem>();
                bool isInputValid = true;
                var errorMsg = "";

                var pairs = input.Split(';', StringSplitOptions.RemoveEmptyEntries);
                if (pairs.Length == 0) continue;

                foreach (var pair in pairs)
                {
                    var parts = pair.Split(':');
                    if (parts.Length != 2)
                    {
                        isInputValid = false;
                        errorMsg = $"Неверный формат пары: {pair}";
                        break;
                    }

                    string articleInput = parts[0].Trim();
                    string quantityInput = parts[1].Trim();

                    var foundDish = menuItems.FirstOrDefault(x => x.Article.Equals(articleInput, StringComparison.OrdinalIgnoreCase));
                    if (foundDish == null)
                    {
                        isInputValid = false;
                        errorMsg = $"Блюдо с кодом '{articleInput}' не найдено.";
                        break;
                    }

                    quantityInput = quantityInput.Replace(',', '.');

                    if (!decimal.TryParse(quantityInput, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal qty) || qty <= 0)
                    {
                        isInputValid = false;
                        errorMsg = $"Некорректное количество для '{articleInput}': {parts[1]}";
                        break;
                    }

                    parsedItems.Add(new OrderItem
                    {
                        MenuItemId = foundDish.Id, 
                        Quantity = qty
                    });
                }

                if (isInputValid)
                {
                    order.Items = parsedItems;
                    break; 
                }
                else
                {
                    logger.WriteError($"Ошибка ввода: {errorMsg}. Попробуйте снова.");
                }
            }

            // === Отправка заказа ===
            logger.WriteLine($"\nОтправка заказа {order.Id} ({order.Items.Count} позиций)...");
            var sendResult = await client.SendOrderAsync(order);

            // === Вывод результата ===
            if (sendResult.IsSuccess)
            {
                logger.WriteLine("УСПЕХ");
            }
            else
            {
                logger.WriteError($"Ошибка отправки заказа: {sendResult.ErrorMessage}");
            }

            logger.WriteLine("Нажмите Enter для выхода...");
            Console.ReadLine();
        }
    }
}
