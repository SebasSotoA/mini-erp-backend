namespace InventoryBack.Application.DTOs;

/// <summary>
/// DTO representing a product in responses.
/// </summary>
public class ProductDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string UnidadMedida { get; set; } = string.Empty;
    public decimal PrecioBase { get; set; }
    public decimal? ImpuestoPorcentaje { get; set; }
    public decimal CostoInicial { get; set; }
    public Guid? CategoriaId { get; set; }
    public string? CodigoSku { get; set; }
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string? ImagenProductoUrl { get; set; }
    public List<ProductoBodegaResponseDto>? Bodegas { get; set; }
}

/// <summary>
/// DTO for ProductoBodega in responses.
/// </summary>
public class ProductoBodegaResponseDto
{
    public Guid BodegaId { get; set; }
    public string? BodegaNombre { get; set; }
    public int CantidadInicial { get; set; }
    public int? CantidadMinima { get; set; }
    public int? CantidadMaxima { get; set; }
}
