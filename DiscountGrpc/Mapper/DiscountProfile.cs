using AutoMapper;
using DiscountGrpc.Protos;

namespace DiscountGrpc.Mapper
{
    public class DiscountProfile : Profile
    {
        public DiscountProfile()
        {
            CreateMap<Models.Discount, DiscountModel>().ReverseMap();
        }
    }
}