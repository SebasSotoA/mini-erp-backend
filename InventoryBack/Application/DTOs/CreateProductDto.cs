using Microsoft.AspNetCore.Http;

namespace InventoryBack.Application.DTOs;

/// <summary>
/// DTO for creating a new product. Supports both quick and advanced creation flows.
/// </summary>
public class CreateProductDto
{
    // ========== REQUIRED FIELDS (Quick Flow) ==========
    
    /// <summary>
    /// Product name (required)
    /// </summary>
    public string Nombre { get; set; } = string.Empty;
    
    /// <summary>
    /// Unit of measure (e.g., "Unidad", "Kilogramo", "Metro")
    /// </summary>
    public string UnidadMedida { get; set; } = string.Empty;
    
    /// <summary>
    /// Main warehouse ID where the product will be stored
    /// </summary>
    public Guid BodegaId { get; set; }
    
    /// <summary>
    /// Base price (must be greater than CostoInicial)
    /// </summary>
    public decimal PrecioBase { get; set; }
    
    /// <summary>
    /// Initial cost (must be less than PrecioBase)
    /// </summary>
    public decimal CostoInicial { get; set; }
    
    /// <summary>
    /// Initial quantity in the main warehouse
    /// </summary>
    public int Cantidad { get; set; }
    
    // ========== OPTIONAL FIELDS (Advanced Flow) ==========
    
    /// <summary>
    /// Tax percentage (0-100). Optional.
    /// </summary>
    public decimal? ImpuestoPorcentaje { get; set; }
    
    /// <summary>
    /// Category ID. Optional.
    /// </summary>
    public Guid? CategoriaId { get; set; }
    
    /// <summary>
    /// SKU code. If not provided, will be auto-generated.
    /// </summary>
    public string? CodigoSku { get; set; }
    
    /// <summary>
    /// Product description. Optional.
    /// </summary>
    public string? Descripcion { get; set; }
    
    /// <summary>
    /// Product image file. Will be uploaded to Supabase Storage.
    /// </summary>
    public IFormFile? Imagen { get; set; }
    
    /// <summary>
    /// Additional warehouses to add the product to.
    /// Each entry includes BodegaId and Cantidad.
    /// </summary>
    public List<ProductoBodegaDto>? BodegasAdicionales { get; set; }
    
    /// <summary>
    /// Additional dynamic fields (key-value pairs).
    /// </summary>
    public List<CampoAdicionalDto>? CamposAdicionales { get; set; }
}

/// <summary>
/// DTO for adding a product to an additional warehouse.
/// </summary>
public class ProductoBodegaDto
{
    /// <summary>
    /// Warehouse ID
    /// </summary>
    public Guid BodegaId { get; set; }
    
    /// <summary>
    /// Initial quantity in this warehouse
    /// </summary>
    public int Cantidad { get; set; }
    
    /// <summary>
    /// Minimum quantity threshold (optional)
    /// </summary>
    public int? CantidadMinima { get; set; }
    
    /// <summary>
    /// Maximum quantity threshold (optional)
    /// </summary>
    public int? CantidadMaxima { get; set; }
}

/// <summary>
/// DTO for dynamic additional fields.
/// </summary>
public class CampoAdicionalDto
{
    /// <summary>
    /// Field name/key
    /// </summary>
    public string Nombre { get; set; } = string.Empty;
    
    /// <summary>
    /// Field value
    /// </summary>
    public string Valor { get; set; } = string.Empty;
}
