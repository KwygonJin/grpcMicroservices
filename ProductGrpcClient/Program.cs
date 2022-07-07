using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using ProductGrpc.Protos;

namespace ProductGrpcClient
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Console.WriteLine("Waiting server...");
            Thread.Sleep(3000);

            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new ProductProtoService.ProductProtoServiceClient(channel);

            await AddProductAsync(client);

            await UpdateProductAsync(client);
            await DeleteProductAsync(client);
            await InsertBulkProductAsync(client);

            await GetProductAsync(client);
            await GetAllProductsAsync(client);

            Console.ReadKey();
        }

        private static async Task InsertBulkProductAsync(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("InsertBulkProductAsync started...");
            using var clientBulk = client.InsertBulkProduct();
            for (var i = 0; i < 3; i++)
            {
                var productModel = new ProductModel
                {
                    Name = "Product " + i,
                    Description = "Bulk inserted product",
                    Price = 299,
                    Status = ProductStatus.Instock,
                    CreatedTime = Timestamp.FromDateTime(DateTime.UtcNow)
                };
                await clientBulk.RequestStream.WriteAsync(productModel);
            }

            await clientBulk.RequestStream.CompleteAsync();

            var responseBulk = await clientBulk;

            Console.WriteLine(
                $"InsertBulkProductAsync Status : {responseBulk.Success}. Insert Count: {responseBulk.InsertCount}");
        }

        private static async Task DeleteProductAsync(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("DeleteProductAsync started...");
            var response = await client.DeleteProductAsync(
                new DeleteProductRequest
                {
                    ProductId = 3
                }
            );
            Console.WriteLine("DeleteProductAsync Response : " + response.Success);
        }

        private static async Task UpdateProductAsync(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("UpdateProductAsync started...");
            var response = await client.UpdateProductAsync(
                new UpdateProductRequest
                {
                    Product = new ProductModel
                    {
                        ProductId = 1,
                        Name = "Red",
                        Description = "New Red Phone Mi10T",
                        Price = 999,
                        Status = ProductStatus.Instock,
                        CreatedTime = Timestamp.FromDateTime(DateTime.UtcNow)
                    }
                }
            );
            Console.WriteLine("UpdateProductAsync Response : " + response);
        }

        private static async Task AddProductAsync(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("AddProductAsync started...");
            var response = await client.AddProductAsync(
                new AddProductRequest
                {
                    Product = new ProductModel
                    {
                        Name = "Red",
                        Description = "New Red Phone Mi10T",
                        Price = 999,
                        Status = ProductStatus.Instock,
                        CreatedTime = Timestamp.FromDateTime(DateTime.UtcNow)
                    }
                }
            );
            Console.WriteLine("AddProductAsync Response : " + response);
        }

        private static async Task GetAllProductsAsync(ProductProtoService.ProductProtoServiceClient client)
        {
            //GetAllProducts
            //Console.WriteLine("GetAllProducts started...");
            //using (var clientData = client.GetAllProducts(new GetAllProductsRequest()))
            //{
            //    while (await clientData.ResponseStream.MoveNext(new CancellationToken()))
            //    {
            //        var currentProduct = clientData.ResponseStream.Current;
            //        Console.WriteLine(currentProduct);
            //    }
            //}
            //Console.WriteLine("GetAllProducts finished");

            Console.WriteLine("GetAllProducts C#9 started...");
            using var clientData = client.GetAllProducts(new GetAllProductsRequest());
            await foreach (var responseData in clientData.ResponseStream.ReadAllAsync())
            {
                Console.WriteLine(responseData);
            }

            Console.WriteLine("GetAllProducts C#9 finished");
        }

        private static async Task GetProductAsync(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("GetProductAsync started...");
            var response = await client.GetProductAsync(
                new GetProductRequest
                {
                    ProductId = 1
                }
            );
            Console.WriteLine("GetProductAsync Response : " + response);
        }
    }
}