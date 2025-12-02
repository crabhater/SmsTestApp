using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using Sms.Test.Core.Models;

namespace Sms.Test.ConsoleApp.Data
{
    public class DatabaseService
    {
        private readonly string _connectionString;
        private readonly Services.AppLogger _logger;

        public DatabaseService(string connectionString, Services.AppLogger logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            var builder = new NpgsqlConnectionStringBuilder(_connectionString);
            var targetDbName = builder.Database;

            if (string.IsNullOrEmpty(targetDbName))
            {
                throw new Exception("В строке подключения не указано имя базы данных (Database=...).");
            }

            builder.Database = "postgres";
            var systemConnString = builder.ToString();

            try
            {
                await using var sysConn = new NpgsqlConnection(systemConnString);
                await sysConn.OpenAsync();


                var checkCmd = new NpgsqlCommand($"SELECT 1 FROM pg_database WHERE datname = '{targetDbName}'", sysConn);
                var exists = await checkCmd.ExecuteScalarAsync() != null;

                if (!exists)
                {
                    _logger.WriteLine($"База данных '{targetDbName}' не найдена. Создаем...");

                    var createCmd = new NpgsqlCommand($"CREATE DATABASE \"{targetDbName}\"", sysConn);
                    await createCmd.ExecuteNonQueryAsync();

                    _logger.WriteLine($"База данных '{targetDbName}' успешно создана.");
                }
                else
                {
                    _logger.WriteLine($"База данных '{targetDbName}' уже существует.");
                }
            }
            catch (Exception ex)
            {
                _logger.WriteError($"Ошибка при создании БД: {ex.Message}");
                throw; 
            }

            await CreateTablesAsync();
        }

        private async Task CreateTablesAsync()
        {
            const string createTableSql = @"
            CREATE TABLE IF NOT EXISTS menu_items (
                id TEXT PRIMARY KEY,
                article TEXT NOT NULL,
                name TEXT NOT NULL,
                price DECIMAL NOT NULL,
                is_weighted BOOLEAN NOT NULL,
                full_path TEXT,
                barcodes TEXT[]
            );";

            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                await using var cmd = new NpgsqlCommand(createTableSql, conn);
                await cmd.ExecuteNonQueryAsync();

                _logger.WriteLine("Таблица 'menu_items' проверена/инициализирована.");
            }
            catch (Exception ex)
            {
                _logger.WriteError($"Ошибка инициализации таблицы: {ex.Message}");
                throw;
            }
        }

        public async Task SaveMenuItemsAsync(List<MenuItem> items)
        {
            if (!items.Any()) return;

            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                await using var transaction = await conn.BeginTransactionAsync();

                await using (var cmdTruncate = new NpgsqlCommand("TRUNCATE TABLE menu_items", conn, transaction))
                {
                    await cmdTruncate.ExecuteNonQueryAsync();
                }

                const string insertSql = @"
                INSERT INTO menu_items (id, article, name, price, is_weighted, full_path, barcodes)
                VALUES (@id, @art, @name, @price, @weight, @path, @barcodes)";

                foreach (var item in items)
                {
                    await using var cmd = new NpgsqlCommand(insertSql, conn, transaction);
                    cmd.Parameters.AddWithValue("id", item.Id);
                    cmd.Parameters.AddWithValue("art", item.Article);
                    cmd.Parameters.AddWithValue("name", item.Name);
                    cmd.Parameters.AddWithValue("price", item.Price);
                    cmd.Parameters.AddWithValue("weight", item.IsWeighted);
                    cmd.Parameters.AddWithValue("path", item.FullPath);
                    cmd.Parameters.AddWithValue("barcodes", (object?)item.Barcodes?.ToArray() ?? DBNull.Value);

                    await cmd.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
                _logger.WriteLine($"В базу данных сохранено {items.Count} записей.");
            }
            catch (Exception ex)
            {
                _logger.WriteError($"Ошибка записи данных: {ex.Message}");
            }
        }
    }
}
