using AutoMapper;
using Mango.Services.OrderAPI.Messages;
using Mango.Services.OrderAPI.Models;

namespace Mango.Services.OrderAPI
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(config =>
            {
                config.CreateMap<CartDetailsDto, OrderDetails>()
                    .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                    .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product.Price));

                config.CreateMap<CheckoutHeaderDto, OrderHeader>()
                    .ForMember(dest => dest.OrderDetails, opt => opt.MapFrom(src => src.CartDetails));
            });

            return mappingConfig;
        }
    }
}
