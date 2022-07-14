using AutoMapper;
using Grpc.Core;
using IdentityModel.Client;
using ProductGrpc.Protos;
using ShoppingCartGrpc.Protos;
using WebInterfaceDemoGRPC.Interfaces;
using WebInterfaceDemoGRPC.ViewModels;

namespace WebInterfaceDemoGRPC.Services;

public class ShoppingCartService : IShoppingCartService
{
    private readonly ShoppingCartProtoService.ShoppingCartProtoServiceClient _shoppingCartProtoService;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;

    public ShoppingCartService(ShoppingCartProtoService.ShoppingCartProtoServiceClient shoppingCartProtoService,
        IMapper mapper,
        IConfiguration configuration)
    {
        _shoppingCartProtoService = shoppingCartProtoService;
        _mapper = mapper;
        _configuration = configuration;
    }

    public async Task<ShoppingCartViewModel> GetCurrentShoppingCartAsync()
    {
        var token = await GetTokenFromIS4();
        var headers = new Metadata();
        headers.Add("Authorization", $"Bearer  {token}");

        await CreateShoppingCartIfNotExistAsync(token);

        ShoppingCartViewModel shoppingCartViewModel = new ShoppingCartViewModel();
        var clientData = await _shoppingCartProtoService.GetShoppingCartAsyncAsync(new GetShoppingCartRequest
        {
            Username = _configuration.GetValue<string>("MainService:UserName")
        }, headers);

        shoppingCartViewModel.UserName = clientData.Username;
        shoppingCartViewModel.Items =
            clientData.Items.Select(i => _mapper.Map<ShoppingCartItemViewModel>(i)).ToList();

        return shoppingCartViewModel;
    }

    public async Task AddToShoppingCartAsync(ProductModel productModel)
    {
        var token = await GetTokenFromIS4();
        await CreateShoppingCartIfNotExistAsync(token);

        using var scClientStream = _shoppingCartProtoService.AddItemIntoShoppingCartAsync();

        var shoppingCartItemModel = _mapper.Map<ShoppingCartItemModel>(productModel);
        var addNewScItem = new AddItemIntoShoppingCartRequest
        {
            Username = _configuration.GetValue<string>("MainService:UserName"),
            DiscountCode = _configuration.GetValue<string>("MainService:DefaultDiscount"),
            NewCartItem = _mapper.Map<ShoppingCartItemModel>(productModel)
        };
        await scClientStream.RequestStream.WriteAsync(addNewScItem);

        await scClientStream.RequestStream.CompleteAsync();

        var addItemIntoScResponse = await scClientStream;
    }

    public async Task DeleteShoppingCartItemAsync(ProductModel productModel)
    {
        var token = await GetTokenFromIS4();

        var result = await _shoppingCartProtoService.RemoveItemIntoShoppingCartAsyncAsync(
            new RemoveItemIntoShoppingCartRequest
            {
                Username = _configuration.GetValue<string>("MainService:UserName"),
                RemoveCartItem = _mapper.Map<ShoppingCartItemModel>(productModel)
            });
    }

    private async Task<string> GetTokenFromIS4()
    {
        var client = new HttpClient();
        var disco = await client.GetDiscoveryDocumentAsync(
            _configuration.GetValue<string>("MainService:IdentityServerUrl"));

        if (disco.IsError)
        {
            Console.WriteLine(disco.Error);
            return string.Empty;
        }

        var tokenResponse = await client.RequestClientCredentialsTokenAsync(
            new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = _configuration.GetValue<string>("MainService:ClientId"),
                ClientSecret = _configuration.GetValue<string>("MainService:ClientSecret"),
                Scope = _configuration.GetValue<string>("MainService:Scope")
            }
        );

        if (tokenResponse.IsError)
        {
            return string.Empty;
        }

        return tokenResponse.AccessToken;
    }

    private async Task CreateShoppingCartIfNotExistAsync(string token)
    {
        var headers = new Metadata();
        headers.Add("Authorization", $"Bearer  {token}");

        try
        {
            await _shoppingCartProtoService.GetShoppingCartAsyncAsync(new GetShoppingCartRequest
                {
                    Username = _configuration.GetValue<string>("MainService:UserName")
                },
                headers);
        }
        catch (RpcException ex)
        {
            if (ex.StatusCode == StatusCode.NotFound)
            {
                await _shoppingCartProtoService.CreateShoppingCartAsyncAsync(new ShoppingCartModel
                    {
                        Username = _configuration.GetValue<string>("MainService:UserName")
                    },
                    headers);
            }
            else
            {
                throw ex;
            }
        }

        ;
    }
}