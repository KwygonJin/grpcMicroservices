using System.Collections.Generic;
using DiscountGrpc.Models;

namespace DiscountGrpc.Data
{
    public class DiscountContext
    {
        public static readonly List<Discount> Discounts = new()
        {
            new() { DiscountId = 1, Code = "CODE_100", Amount = 100 },
            new() { DiscountId = 2, Code = "CODE_200", Amount = 200 },
            new() { DiscountId = 3, Code = "CODE_300", Amount = 300 }
        };
    }
}