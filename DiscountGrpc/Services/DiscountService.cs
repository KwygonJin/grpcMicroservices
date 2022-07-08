using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DiscountGrpc.Data;
using DiscountGrpc.Protos;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace DiscountGrpc.Services
{
    public class DiscountService : DiscountProtoService.DiscountProtoServiceBase
    {
        private readonly ILogger<DiscountService> _logger;
        private readonly DiscountContext _discountContext;
        private readonly IMapper _mapper;

        public DiscountService(ILogger<DiscountService> logger, DiscountContext context, IMapper mapper)
        {
            _logger = logger;
            _discountContext = context;
            _mapper = mapper;
        }

        public override Task<DiscountModel> GetDiscount(GetDiscountRequest request
            , ServerCallContext context)
        {
            var discount = DiscountContext.Discounts.FirstOrDefault(d => d.Code == request.DiscountCode);
            if (discount == null)
            {
                var message = $"Invalid DiscountCode for Discount. DiscountCode : {request.DiscountCode}";
                _logger.LogError(message);
                throw new RpcException(new Status(StatusCode.NotFound,
                    message));
            }

            var discountModel = _mapper.Map<DiscountModel>(discount);
            return Task.FromResult(discountModel);
        }
    }
}