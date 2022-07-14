using AutoMapper;
using Grpc.Core;
using ProductGrpc.Protos;
using WebInterfaceDemoGRPC.Interfaces;
using WebInterfaceDemoGRPC.ViewModels;

namespace WebInterfaceDemoGRPC.Services;

public class ProductService : IProductService
{
    private readonly ProductProtoService.ProductProtoServiceClient _productProtoService;
    private readonly IMapper _mapper;

    public ProductService(ProductProtoService.ProductProtoServiceClient productProtoService, IMapper mapper)
    {
        _productProtoService = productProtoService;
        _mapper = mapper;
    }

    public async Task<ProductListViewModel> GetAllProductsAsync()
    {
        ProductListViewModel prodListViewModel = new ProductListViewModel();
        prodListViewModel.AllProducts = new List<ProductViewModel>();
        using var clientData = _productProtoService.GetAllProductsAsync(new GetAllProductsRequest());
        await foreach (var responseData in clientData.ResponseStream.ReadAllAsync())
        {
            prodListViewModel.AllProducts.Add(_mapper.Map<ProductViewModel>(responseData));
        }

        return prodListViewModel;
    }

    public async Task<ProductModel> GetProductByIdAsync(int productId)
    {
        var productModel = await _productProtoService.GetProductAsyncAsync(new GetProductRequest
        {
            ProductId = productId
        });

        return productModel;
    }

    public async Task AddProductToDbAsync(ProductViewModel productViewModel)
    {
        await _productProtoService.AddProductAsyncAsync(new AddProductRequest
        {
            Product = _mapper.Map<ProductModel>(productViewModel)
        });
    }
}