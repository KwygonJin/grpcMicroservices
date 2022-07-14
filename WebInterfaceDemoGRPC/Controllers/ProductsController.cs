using Microsoft.AspNetCore.Mvc;
using WebInterfaceDemoGRPC.Interfaces;
using WebInterfaceDemoGRPC.ViewModels;

namespace WebInterfaceDemoGRPC.Controllers;

public class ProductsController : Controller
{
    private readonly IProductService _productService;
    private readonly IShoppingCartService _shoppingCartService;

    public ProductsController(IProductService productService, IShoppingCartService shoppingCartService)
    {
        _productService = productService;
        _shoppingCartService = shoppingCartService;
    }

    //[Route("Products/List")]
    public async Task<ViewResult> List()
    {
        var products = await _productService.GetAllProductsAsync();
        return View(products);
    }

    //[Route("Products/Add")]
    public IActionResult Add()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddProductToDb(ProductViewModel productViewModel)
    {
        await _productService.AddProductToDbAsync(productViewModel);
        return RedirectToAction("List");
    }

    public async Task<IActionResult> AddProductToCart(int productId)
    {
        var productModel = await _productService.GetProductByIdAsync(productId);
        await _shoppingCartService.AddToShoppingCartAsync(productModel);
        return RedirectToAction("List");
    }
}