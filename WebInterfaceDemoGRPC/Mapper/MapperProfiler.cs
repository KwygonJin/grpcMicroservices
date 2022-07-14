using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using ProductGrpc.Protos;
using ShoppingCartGrpc.Protos;
using WebInterfaceDemoGRPC.ViewModels;

namespace WebInterfaceDemoGRPC.Mapper;

public class MapperProfiler : Profile
{
    public MapperProfiler()
    {
        int defaultQuantity = 1;

        CreateMap<ProductModel, ProductViewModel>()
            .ReverseMap()
            .ForPath(d => d.CreatedTime, opt => opt.MapFrom(src => Timestamp.FromDateTime(DateTime.UtcNow)));
        CreateMap<ShoppingCartModel, ShoppingCartViewModel>().ReverseMap();
        CreateMap<ShoppingCartItemModel, ShoppingCartItemViewModel>()
            .ForMember(d => d.Sum, o => o.MapFrom(src => src.Price * src.Quantity))
            .ReverseMap();
        CreateMap<ProductModel, ShoppingCartItemModel>()
            .ForMember(d => d.Productname, o => o.MapFrom(src => src.Name))
            .ForMember(d => d.Quantity, o => o.MapFrom(src => defaultQuantity));
    }
}