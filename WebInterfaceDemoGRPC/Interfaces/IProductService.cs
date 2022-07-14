using ProductGrpc.Protos;
using WebInterfaceDemoGRPC.ViewModels;

namespace WebInterfaceDemoGRPC.Interfaces;

public interface IProductService
{
    Task<ProductListViewModel> GetAllProductsAsync();

    Task<ProductModel> GetProductByIdAsync(int productId);

    Task AddProductToDbAsync(ProductViewModel productViewModel);
}