using ProductGrpc.Protos;
using WebInterfaceDemoGRPC.ViewModels;

namespace WebInterfaceDemoGRPC.Interfaces;

public interface IShoppingCartService
{
    Task<ShoppingCartViewModel> GetCurrentShoppingCartAsync();

    Task AddToShoppingCartAsync(ProductModel productModel);

    Task DeleteShoppingCartItemAsync(ProductModel productModel);
}