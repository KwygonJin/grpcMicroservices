using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProductGrpc.Protos;

namespace ProductWorkerService
{
    public class ProductFactory
    {
        private readonly ILogger<ProductFactory> _logger;
        private readonly IConfiguration _config;

        public ProductFactory(ILogger<ProductFactory> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public Task<AddProductRequest> Generate()
        {
            var productName = $"{_config.GetValue<string>("WorkerService:ProductName")}_{DateTime.Now}";
            var productRequest = new AddProductRequest
            {
                Product = new ProductModel
                {
                    Name = productName,
                    Description = $"{productName}_Description",
                    Price = new Random().Next(1000),
                    Status = ProductStatus.Instock,
                    CreatedTime = Timestamp.FromDateTime(DateTime.UtcNow)
                }
            };

            return Task.FromResult(productRequest);
        }
    }
}