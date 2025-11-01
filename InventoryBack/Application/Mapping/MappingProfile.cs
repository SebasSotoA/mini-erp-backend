using AutoMapper;
using InventoryBack.Application.DTOs;
using InventoryBack.Domain.Entities;

namespace InventoryBack.Application.Mapping;

/// <summary>
/// AutoMapper profile for entity to DTO mappings.
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Producto mappings
        CreateMap<Producto, ProductDto>();
        
        CreateMap<CreateProductDto, Producto>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => true));

        CreateMap<UpdateProductDto, Producto>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore());

        // CampoExtra mappings
        CreateMap<CampoExtra, CampoExtraDto>();
        
        CreateMap<CreateCampoExtraDto, CampoExtra>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => true));

        CreateMap<UpdateCampoExtraDto, CampoExtra>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.Activo, opt => opt.Ignore());

        // Bodega mappings
        CreateMap<Bodega, BodegaDto>();
        
        CreateMap<CreateBodegaDto, Bodega>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => true));

        CreateMap<UpdateBodegaDto, Bodega>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.Activo, opt => opt.Ignore());
    }
}
