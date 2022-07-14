namespace WebInterfaceDemoGRPC.ViewModels;

public class ShoppingCartItemViewModel
{
    public int Quantity { get; set; }
    public float Price { get; set; }
    public float Sum { get; set; }

    public int ProductId { get; set; }
    public string ProductName { get; set; }
}