using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShoppingCartGrpc.Data;
using ShoppingCartGrpc.Models;
using ShoppingCartGrpc.Protos;

namespace ShoppingCartGrpc.Services
{
    [Authorize]
    public class ShoppingCartService : ShoppingCartProtoService.ShoppingCartProtoServiceBase
    {
        private readonly ILogger<ShoppingCartService> _logger;
        private readonly ShoppingCartContext _shoppingCartContext;
        private readonly IMapper _mapper;
        private readonly DiscountService _discountService;

        public ShoppingCartService(ILogger<ShoppingCartService> logger, ShoppingCartContext context, IMapper mapper,
            DiscountService discountService)
        {
            _logger = logger;
            _shoppingCartContext = context;
            _mapper = mapper;
            _discountService = discountService;
        }

        public override async Task<ShoppingCartModel> GetShoppingCartAsync(GetShoppingCartRequest request,
            ServerCallContext context)
        {
            var shoppingCart =
                await _shoppingCartContext.ShoppingCarts
                    .Include(sc => sc.Items)
                    .FirstOrDefaultAsync(s => s.UserName == request.Username);
            if (shoppingCart == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Shopping cart with User={request.Username} is not found!"));
            }

            var shoppingCartModel = _mapper.Map<ShoppingCartModel>(shoppingCart);
            shoppingCartModel.Items.Clear();
            shoppingCartModel.Items.Add(shoppingCart.Items.Select(i => _mapper.Map<ShoppingCartItemModel>(i)).ToList());
            return shoppingCartModel;
        }

        public override async Task<ShoppingCartModel> CreateShoppingCartAsync(ShoppingCartModel request,
            ServerCallContext context)
        {
            ShoppingCartModel shoppingCartModel;

            var shoppingCart = _mapper.Map<ShoppingCart>(request);
            var isExist = await _shoppingCartContext.ShoppingCarts
                .AnyAsync(s => s.UserName == shoppingCart.UserName);

            if (isExist)
            {
                shoppingCartModel = _mapper.Map<ShoppingCartModel>(shoppingCart);
                return shoppingCartModel;
            }

            _shoppingCartContext.ShoppingCarts.Add(shoppingCart);
            await _shoppingCartContext.SaveChangesAsync();

            _logger.LogInformation("Shopping cart is successfully created for UserName : {userName}",
                shoppingCart.UserName);

            shoppingCartModel = _mapper.Map<ShoppingCartModel>(shoppingCart);
            return shoppingCartModel;
        }

        [AllowAnonymous]
        public override async Task<RemoveItemIntoShoppingCartResponse> RemoveItemIntoShoppingCartAsync
            (RemoveItemIntoShoppingCartRequest request, ServerCallContext context)
        {
            var shoppingCart =
                await _shoppingCartContext.ShoppingCarts.FirstOrDefaultAsync(s => s.UserName == request.Username);
            if (shoppingCart == null)
            {
                _logger.LogError("Invalid Username for Shopping cart removing. UserName : {userName}",
                    shoppingCart.UserName);
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Shopping cart with User={request.Username} is not found!"));
            }

            var removeCartItem = await _shoppingCartContext.ShoppingCartItems
                .FirstOrDefaultAsync(i => i.ProductId == request.RemoveCartItem.ProductId
                                          && i.ShoppingCartId == shoppingCart.Id);
            if (removeCartItem == null)
            {
                _logger.LogError("Cart item with ProductId={ProductId} is not found!",
                    request.RemoveCartItem.ProductId);
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Cart item with ProductId={request.RemoveCartItem.ProductId} is not found!"));
            }

            _shoppingCartContext.ShoppingCartItems.Remove(removeCartItem);
            var removeCount = await _shoppingCartContext.SaveChangesAsync();

            return new RemoveItemIntoShoppingCartResponse
            {
                Success = removeCount > 0
            };
        }

        [AllowAnonymous]
        public override async Task<AddItemIntoShoppingCartResponse> AddItemIntoShoppingCartAsync
            (IAsyncStreamReader<AddItemIntoShoppingCartRequest> requestStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var shoppingCart =
                    await _shoppingCartContext.ShoppingCarts
                        .FirstOrDefaultAsync(s => s.UserName == requestStream.Current.Username);
                if (shoppingCart == null)
                {
                    _logger.LogError("Invalid Username for Shopping cart removing. UserName : {userName}",
                        shoppingCart.UserName);
                    throw new RpcException(new Status(StatusCode.NotFound,
                        $"Shopping cart with User={requestStream.Current.Username} is not found!"));
                }

                var newAddedCartItem = _mapper.Map<ShoppingCartItem>(requestStream.Current.NewCartItem);

                var cartItem = await _shoppingCartContext.ShoppingCartItems
                    .FirstOrDefaultAsync(i => i.ProductId == newAddedCartItem.ProductId
                                              && i.ShoppingCartId == shoppingCart.Id);
                if (cartItem != null)
                {
                    cartItem.Quantity++;
                }
                else
                {
                    var discount = await _discountService.GetDiscountAsync(requestStream.Current.DiscountCode);
                    newAddedCartItem.Price -= discount.Amount;
                    newAddedCartItem.ShoppingCartId = shoppingCart.Id;

                    _shoppingCartContext.ShoppingCartItems.Add(newAddedCartItem);
                }
            }

            var insertCount = await _shoppingCartContext.SaveChangesAsync();

            return new AddItemIntoShoppingCartResponse
            {
                Success = insertCount > 0,
                InsertCount = insertCount
            };
        }
    }
}