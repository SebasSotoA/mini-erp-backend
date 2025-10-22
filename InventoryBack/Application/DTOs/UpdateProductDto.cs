namespace InventoryBack.Application.DTOs;

/// <summary>
/// DTO for updating an existing product.
/// </summary>
public class UpdateProductDto
{
    public string Nombre { get; set; } = string.Empty;
    public string UnidadMedida { get; set; } = string.Empty;
    public decimal PrecioBase { get; set; }
    public decimal? ImpuestoPorcentaje { get; set; }
    public decimal CostoInicial { get; set; }
    public Guid? CategoriaId { get; set; }
    public string? CodigoSku { get; set; }
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }
    public string? ImagenProductoUrl { get; set; }
}
