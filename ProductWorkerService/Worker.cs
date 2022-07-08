using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductGrpc.Protos;

namespace ProductWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _config;
        private readonly ProductFactory _factory;

        public Worker(ILogger<Worker> logger, IConfiguration config, ProductFactory factory)
        {
            _logger = logger;
            _config = config;
            _factory = factory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Waiting server...");
            Thread.Sleep(3000);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                using var channel = GrpcChannel.ForAddress(_config.GetValue<string>("WorkerService:ServerURL"));
                var client = new ProductProtoService.ProductProtoServiceClient(channel);

                _logger.LogInformation("AddProductAsync started...");
                var response = await client.AddProductAsyncAsync(await _factory.Generate());
                _logger.LogInformation("AddProductAsync Response : " + response);

                await Task.Delay(_config.GetValue<int>("WorkerService:TaskInterval"), stoppingToken);
            }
        }
    }
}