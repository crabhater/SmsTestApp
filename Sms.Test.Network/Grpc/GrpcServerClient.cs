using Grpc.Net.Client;
using Sms.Test.Core.Common;
using Sms.Test.Core.Interfaces;
using CoreModels = Sms.Test.Core.Models;
using Sms.Test.Grpc.Generated;
using Google.Protobuf.WellKnownTypes;

namespace Sms.Test.Network.Grpc
{

    public class GrpcServerClient : IServerClient
    {
        private readonly string _address;

        public GrpcServerClient(string address)
        {
            _address = address;
        }

        public async Task<OperationResult<List<CoreModels.MenuItem>>> GetMenuAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_address);
                var client = new SmsTestService.SmsTestServiceClient(channel);

                var request = new BoolValue { Value = true };

                var response = await client.GetMenuAsync(request, cancellationToken: cancellationToken);

                if (!response.Success)
                {
                    return OperationResult<List<CoreModels.MenuItem>>.Failure(response.ErrorMessage);
                }

                var coreItems = response.MenuItems.Select(gItem => new CoreModels.MenuItem
                {
                    Id = gItem.Id,
                    Article = gItem.Article,
                    Name = gItem.Name,
                    Price = (decimal)gItem.Price,
                    IsWeighted = gItem.IsWeighted,
                    FullPath = gItem.FullPath,
                    Barcodes = gItem.Barcodes.ToList()
                }).ToList();

                return OperationResult<List<CoreModels.MenuItem>>.Success(coreItems);
            }
            catch (Exception ex)
            {
                return OperationResult<List<CoreModels.MenuItem>>.Failure($"gRPC Ошибка: {ex.Message}");
            }
        }

        public async Task<OperationResult> SendOrderAsync(CoreModels.Order order, CancellationToken cancellationToken = default)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_address);
                var client = new SmsTestService.SmsTestServiceClient(channel);

                var grpcOrder = new Order
                {
                    Id = order.Id.ToString(),
                };

                foreach (var item in order.Items)
                {
                    grpcOrder.OrderItems.Add(new OrderItem
                    {
                        Id = item.MenuItemId,
                        Quantity = (double)item.Quantity
                    });
                }

                var response = await client.SendOrderAsync(grpcOrder, cancellationToken: cancellationToken);

                if (!response.Success)
                {
                    return OperationResult.Failure(response.ErrorMessage);
                }

                return OperationResult.Success();
            }
            catch (Exception ex)
            {
                return OperationResult.Failure($"gRPC Ошибка: {ex.Message}");
            }
        }
    }
}