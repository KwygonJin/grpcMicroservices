using System;
using System.Collections.Generic;
using System.Linq;
using ProductGrpc.Models;

namespace ProductGrpc.Data
{
    public class ProductsContextSeed
    {
        public static async void SeedAsync(ProductsContext productsContext)
        {
            if (!productsContext.Products.Any())
            {
                var products = new List<Product>
                {
                    new()
                    {
                        ProductId = 1,
                        Name = "Mi10T",
                        Description = "New Xiaomi Phone Mi10T",
                        Price = 699,
                        StatusEnum = ProductStatusEnum.INSTOCK,
                        CreatedTime = DateTime.UtcNow
                    },
                    new()
                    {
                        ProductId = 2,
                        Name = "P40",
                        Description = "New Huawei Phone P40",
                        Price = 899,
                        StatusEnum = ProductStatusEnum.INSTOCK,
                        CreatedTime = DateTime.UtcNow
                    },
                    new()
                    {
                        ProductId = 3,
                        Name = "A50",
                        Description = "New Samsung Phone A50",
                        Price = 699,
                        StatusEnum = ProductStatusEnum.INSTOCK,
                        CreatedTime = DateTime.UtcNow
                    }
                };
                productsContext.Products.AddRange(products);
                await productsContext.SaveChangesAsync();
            }
        }
    }
}