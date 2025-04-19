using AutoMapper;
using MiniMart.Application.Models;
using MiniMart.Domain.Models;

namespace MiniMart.API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            CreateMap<ProductCreateRequest, Product>();
            CreateMap<ProductUpdateRequest, Product>();
            CreateMap<Product, ProductResponse>();
            CreateMap<ProductInventory, ProductInventoryResponse>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product != null ? src.Product.Price : default));
            CreateMap<PurchaseOrder, PurchaseOrderResponse>();
        } 
    }
}
