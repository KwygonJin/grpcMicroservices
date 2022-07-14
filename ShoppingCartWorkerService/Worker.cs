using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductGrpc.Protos;
using ShoppingCartGrpc.Protos;

namespace ShoppingCartWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Waiting server...");
            Thread.Sleep(3000);
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scChannel = GrpcChannel.ForAddress
                    (_configuration.GetValue<string>("WorkerService:ShoppingCartServerUrl"));
                var scClient = new ShoppingCartProtoService.ShoppingCartProtoServiceClient(scChannel);

                var token = await GetTokenFromIS4();

                var shoppingCart = await GetOrCreateShoppingCartAsync(scClient, token);

                using var scClientStream = scClient.AddItemIntoShoppingCartAsync();

                using var productChannel = GrpcChannel.ForAddress
                    (_configuration.GetValue<string>("WorkerService:ProductServerUrl"));
                var productClient = new ProductProtoService.ProductProtoServiceClient(productChannel);

                _logger.LogInformation("GetAllProducts started...");
                using var clientData = productClient.GetAllProductsAsync(new GetAllProductsRequest());
                await foreach (var responseData in clientData.ResponseStream.ReadAllAsync())
                {
                    _logger.LogInformation("GetAllProducts response : " + responseData);
                    var addNewScItem = new AddItemIntoShoppingCartRequest
                    {
                        Username = _configuration.GetValue<string>("WorkerService:UserName"),
                        DiscountCode = "CODE_100",
                        NewCartItem = new ShoppingCartItemModel
                        {
                            ProductId = responseData.ProductId,
                            Productname = responseData.Name,
                            Price = responseData.Price,
                            Color = "Black",
                            Quantity = 1
                        }
                    };
                    await scClientStream.RequestStream.WriteAsync(addNewScItem);
                    _logger.LogInformation("Added new information to SC : " + addNewScItem);
                }

                await scClientStream.RequestStream.CompleteAsync();

                var addItemIntoScResponse = await scClientStream;
                _logger.LogInformation("addItemIntoScResponse Client Stream response: " + addItemIntoScResponse);

                await Task.Delay(_configuration.GetValue<int>("WorkerService:TaskInterval"), stoppingToken);
            }
        }

        private async Task<string> GetTokenFromIS4()
        {
            var client = new HttpClient();
            var disco = await client.GetDiscoveryDocumentAsync(
                _configuration.GetValue<string>("WorkerService:IdentityServerUrl"));

            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return string.Empty;
            }

            var tokenResponse = await client.RequestClientCredentialsTokenAsync(
                new ClientCredentialsTokenRequest
                {
                    Address = disco.TokenEndpoint,
                    ClientId = "ShoppingCartClient",
                    ClientSecret = "secret",
                    Scope = "ShoppingCartAPI"
                }
            );

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return string.Empty;
            }

            return tokenResponse.AccessToken;
        }

        private async Task<ShoppingCartModel> GetOrCreateShoppingCartAsync(
            ShoppingCartProtoService.ShoppingCartProtoServiceClient scClient,
            string token)
        {
            ShoppingCartModel shoppingCartModel;
            var headers = new Metadata();
            headers.Add("Authorization", $"Bearer  {token}");

            try
            {
                _logger.LogInformation("GetShoppingCartAsyncAsync started...");

                shoppingCartModel =
                    await scClient.GetShoppingCartAsyncAsync(new GetShoppingCartRequest
                        {
                            Username = _configuration.GetValue<string>("WorkerService:UserName")
                        },
                        headers);
                _logger.LogInformation("GetOrCreateShoppingCartAsync successfully...");
            }
            catch (RpcException ex)
            {
                if (ex.StatusCode == StatusCode.NotFound)
                {
                    _logger.LogInformation("CreateShoppingCartAsyncAsync started...");
                    shoppingCartModel = await scClient.CreateShoppingCartAsyncAsync(new ShoppingCartModel
                        {
                            Username = _configuration.GetValue<string>("WorkerService:UserName")
                        },
                        headers);
                    _logger.LogInformation("CreateShoppingCartAsyncAsync successfully...");
                }
                else
                {
                    throw ex;
                }
            }

            return shoppingCartModel;
        }
    }
}