using System.Threading.Tasks;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductGrpc.Data;
using ProductGrpc.Models;
using ProductGrpc.Protos;
using ProductStatus = ProductGrpc.Protos.ProductStatus;

namespace ProductGrpc.Services
{
    public class ProductService : ProductProtoService.ProductProtoServiceBase
    {
        private readonly ILogger<ProductService> _logger;
        private readonly ProductsContext _productContext;
        private readonly IMapper _mapper;

        public ProductService(ILogger<ProductService> logger, ProductsContext productContext, IMapper mapper)
        {
            _logger = logger;
            _productContext = productContext;
            _mapper = mapper;
        }

        public override Task<Empty> Test(Empty request, ServerCallContext context)
        {
            return base.Test(request, context);
        }

        public override async Task<ProductModel> GetProduct(GetProductRequest request, ServerCallContext context)
        {
            var product = await _productContext.Products.FindAsync(request.ProductId);
            if (product == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Product with ID={request.ProductId} is not found!"));
            }

            var productModel = _mapper.Map<ProductModel>(product);
            productModel.Status = ProductStatus.Instock;

            return productModel;
        }

        public override async Task GetAllProducts(GetAllProductsRequest request,
            IServerStreamWriter<ProductModel> responseStream, ServerCallContext context)
        {
            var productList = await _productContext.Products.ToListAsync();
            foreach (var product in productList)
            {
                var productModel = _mapper.Map<ProductModel>(product);
                productModel.Status = ProductStatus.Instock;
                await responseStream.WriteAsync(productModel);
            }
        }

        public override async Task<ProductModel> AddProduct(AddProductRequest request, ServerCallContext context)
        {
            var product = _mapper.Map<Product>(request.Product);
            product.Status = Models.ProductStatus.INSTOCK;

            _productContext.Products.Add(product);
            await _productContext.SaveChangesAsync();

            _logger.LogInformation($"Product successfully added: {product.Name}_{product.ProductId}");

            var productModel = _mapper.Map<ProductModel>(product);
            productModel.Status = ProductStatus.Instock;

            return productModel;
        }

        public override async Task<ProductModel> UpdateProduct(UpdateProductRequest request, ServerCallContext context)
        {
            var product = _mapper.Map<Product>(request.Product);

            var isExist = await _productContext.Products.AnyAsync(p => p.ProductId == product.ProductId);
            if (!isExist)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Product with ID={product.ProductId} is not found!"));
            }

            _productContext.Entry(product).State = EntityState.Modified;
            await _productContext.SaveChangesAsync();
            var productModel = _mapper.Map<ProductModel>(product);

            return productModel;
        }

        public override async Task<DeleteProductResponse> DeleteProduct(DeleteProductRequest request,
            ServerCallContext context)
        {
            var product = await _productContext.Products.FindAsync(request.ProductId);
            if (product == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Product with ID={request.ProductId} is not found!"));
            }

            _productContext.Products.Remove(product);
            var deleteCount = await _productContext.SaveChangesAsync();

            var response = new DeleteProductResponse
            {
                Success = deleteCount > 0
            };

            return response;
        }

        public override async Task<InsertBulkProductResponse> InsertBulkProduct(
            IAsyncStreamReader<ProductModel> requestStream,
            ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var product = _mapper.Map<Product>(requestStream.Current);
                _productContext.Products.Add(product);
            }

            var insertCount = await _productContext.SaveChangesAsync();
            var response = new InsertBulkProductResponse
            {
                Success = insertCount > 0,
                InsertCount = insertCount
            };

            return response;
        }
    }
}