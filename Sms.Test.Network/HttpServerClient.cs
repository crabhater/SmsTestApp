using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Sms.Test.Core.Common;
using Sms.Test.Core.Interfaces;
using Sms.Test.Core.Models;
using Sms.Test.Network.Http.Models.Dto;
using Sms.Test.Network.Http.Models;

namespace Sms.Test.Network
{
    public class HttpServerClient : IServerClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _endpointUrl;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = null,
            WriteIndented = true
        };

        public HttpServerClient(HttpClient httpClient, string endpointUrl, string username, string password)
        {
            _httpClient = httpClient;
            _endpointUrl = endpointUrl;

            var authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
        }

        public async Task<OperationResult<List<MenuItem>>> GetMenuAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var requestBody = new BaseRequest<GetMenuParams>
                {
                    Command = "GetMenu",
                    CommandParameters = new GetMenuParams { WithPrice = true }
                };

                var response = await _httpClient.PostAsJsonAsync(_endpointUrl, requestBody, _jsonOptions, cancellationToken);

                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<BaseResponse<GetMenuData>>(_jsonOptions, cancellationToken);

                if (result == null)
                    return OperationResult<List<MenuItem>>.Failure("Получен пустой ответ от сервера.");

                if (!result.Success)
                    return OperationResult<List<MenuItem>>.Failure(result.ErrorMessage ?? "Неизвестная ошибка сервера.");

                var menuItems = result.Data?.MenuItems.Select(dto => new MenuItem
                {
                    Id = dto.Id,
                    Article = dto.Article,
                    Name = dto.Name,
                    Price = dto.Price,
                    IsWeighted = dto.IsWeighted,
                    FullPath = dto.FullPath,
                    Barcodes = dto.Barcodes
                }).ToList() ?? new List<MenuItem>();

                return OperationResult<List<MenuItem>>.Success(menuItems);
            }
            catch (Exception ex)
            {
                return OperationResult<List<MenuItem>>.Failure($"HTTP Ошибка: {ex.Message}");
            }
        }

        public async Task<OperationResult> SendOrderAsync(Order order, CancellationToken cancellationToken = default)
        {
            try
            {
                var orderParams = new SendOrderParams
                {
                    OrderId = order.Id.ToString(),
                    MenuItems = order.Items.Select(i => new OrderItemDto
                    {
                        Id = i.MenuItemId,
                        Quantity = i.Quantity.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    }).ToList()
                };

                var requestBody = new BaseRequest<SendOrderParams>
                {
                    Command = "SendOrder",
                    CommandParameters = orderParams
                };

                var response = await _httpClient.PostAsJsonAsync(_endpointUrl, requestBody, _jsonOptions, cancellationToken);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<BaseResponse<object>>(_jsonOptions, cancellationToken);

                if (result == null)
                    return OperationResult.Failure("Получен пустой ответ от сервера.");

                if (!result.Success)
                    return OperationResult.Failure(result.ErrorMessage ?? "Неизвестная ошибка сервера.");

                return OperationResult.Success();
            }
            catch (Exception ex)
            {
                return OperationResult.Failure($"Ошибка: {ex.Message}");
            }
        }
    }
}
