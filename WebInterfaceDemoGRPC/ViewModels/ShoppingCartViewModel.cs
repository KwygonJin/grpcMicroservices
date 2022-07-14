namespace WebInterfaceDemoGRPC.ViewModels;

public class ShoppingCartViewModel
{
    public string UserName { get; set; }
    public List<ShoppingCartItemViewModel> Items { get; set; } = new();
}