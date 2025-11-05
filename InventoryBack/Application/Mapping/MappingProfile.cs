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
        // Note: StockActual and CategoriaNombre will be populated manually in the service layer
        CreateMap<Producto, ProductDto>()
            .ForMember(dest => dest.StockActual, opt => opt.Ignore())  // Set manually after DB query
            .ForMember(dest => dest.CategoriaNombre, opt => opt.Ignore());  // Set manually after DB query
        
        CreateMap<CreateProductDto, Producto>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.BodegaPrincipalId, opt => opt.MapFrom(src => src.BodegaPrincipalId));

        CreateMap<UpdateProductDto, Producto>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.BodegaPrincipalId, opt => opt.Condition(src => src.BodegaPrincipalId.HasValue));

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

        // Categoria mappings
        CreateMap<Categoria, CategoriaDto>();
        
        CreateMap<CreateCategoriaDto, Categoria>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.ImagenCategoriaUrl, opt => opt.Ignore());

        CreateMap<UpdateCategoriaDto, Categoria>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.Activo, opt => opt.Ignore())
            .ForMember(dest => dest.ImagenCategoriaUrl, opt => opt.Ignore());

        // Vendedor mappings
        CreateMap<CreateVendedorDto, Vendedor>();
        CreateMap<UpdateVendedorDto, Vendedor>();
        CreateMap<Vendedor, VendedorDto>();

        // Proveedor mappings
        CreateMap<CreateProveedorDto, Proveedor>();
        CreateMap<UpdateProveedorDto, Proveedor>();
        CreateMap<Proveedor, ProveedorDto>();
    }
}
