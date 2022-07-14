using WebInterfaceDemoGRPC.Enums;

namespace WebInterfaceDemoGRPC.ViewModels;

public class ProductViewModel
{
    public int ProductId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public float Price { get; set; }
    public ProductStatusEnum StatusEnum { get; set; }
}