using System.Collections.Generic;
using System.Linq;
using ShoppingCartGrpc.Models;

namespace ShoppingCartGrpc.Data
{
    public class ShoppingCartContextSeed
    {
        public static async void SeedAsync(ShoppingCartContext shoppingCartContext)
        {
            if (!shoppingCartContext.ShoppingCarts.Any())
            {
                var shoppingCarts = new List<ShoppingCart>
                {
                    new()
                    {
                        UserName = "swn",
                        Items = new List<ShoppingCartItem>
                        {
                            new()
                            {
                                Quantity = 2,
                                Color = "Black",
                                Price = 699,
                                ProductId = 1,
                                ProductName = "Mi10T"
                            },
                            new()
                            {
                                Quantity = 3,
                                Color = "Red",
                                Price = 899,
                                ProductId = 2,
                                ProductName = "P40"
                            }
                        }
                    }
                };
                shoppingCartContext.ShoppingCarts.AddRange(shoppingCarts);
                await shoppingCartContext.SaveChangesAsync();
            }
        }
    }
}