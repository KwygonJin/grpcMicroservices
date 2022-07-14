using Microsoft.AspNetCore.Mvc;
using ProductGrpc.Protos;
using WebInterfaceDemoGRPC.Interfaces;

namespace WebInterfaceDemoGRPC.Controllers;

public class ShopCartController : Controller
{
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IProductService _productService;

    public ShopCartController(IShoppingCartService shoppingCartService, IProductService productService)
    {
        _shoppingCartService = shoppingCartService;
        _productService = productService;
    }

    public async Task<IActionResult> Index()
    {
        var shoppingCart = await _shoppingCartService.GetCurrentShoppingCartAsync();
        return View(shoppingCart);
    }

    public async Task<IActionResult> Delete(int productId)
    {
        ProductModel productModel = await _productService.GetProductByIdAsync(productId);
        await _shoppingCartService.DeleteShoppingCartItemAsync(productModel);
        return RedirectToAction("Index");
    }
}