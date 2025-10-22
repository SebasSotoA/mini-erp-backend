namespace InventoryBack.Application.DTOs;

/// <summary>
/// DTO for creating a new product.
/// </summary>
public class CreateProductDto
{
    public string Nombre { get; set; } = string.Empty;
    public string UnidadMedida { get; set; } = string.Empty;
    public decimal PrecioBase { get; set; }
    public decimal? ImpuestoPorcentaje { get; set; }
    public decimal CostoInicial { get; set; }
    public Guid? CategoriaId { get; set; }
    public string? CodigoSku { get; set; }
    public string? Descripcion { get; set; }
    public string? ImagenProductoUrl { get; set; }
}
