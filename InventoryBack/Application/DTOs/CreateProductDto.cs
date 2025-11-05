using System.Text.Json.Serialization;

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
    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = string.Empty;
    
    /// <summary>
    /// Unit of measure (e.g., "Unidad", "Kilogramo", "Metro")
    /// </summary>
    [JsonPropertyName("unidadMedida")]
    public string UnidadMedida { get; set; } = string.Empty;
    
    /// <summary>
    /// Main warehouse ID where the product will be stored
    /// </summary>
    [JsonPropertyName("bodegaPrincipalId")]
    public Guid BodegaPrincipalId { get; set; }
    
    /// <summary>
    /// Base price (must be greater than CostoInicial)
    /// </summary>
    [JsonPropertyName("precioBase")]
    public decimal PrecioBase { get; set; }
    
    /// <summary>
    /// Initial cost (must be less than PrecioBase)
    /// </summary>
    [JsonPropertyName("costoInicial")]
    public decimal CostoInicial { get; set; }
    
    /// <summary>
    /// Initial quantity in the main warehouse
    /// </summary>
    [JsonPropertyName("cantidadInicial")]
    public int CantidadInicial { get; set; }
    
    /// <summary>
    /// Minimum quantity threshold for main warehouse (optional)
    /// </summary>
    [JsonPropertyName("cantidadMinima")]
    public int? CantidadMinima { get; set; }
    
    /// <summary>
    /// Maximum quantity threshold for main warehouse (optional)
    /// </summary>
    [JsonPropertyName("cantidadMaxima")]
    public int? CantidadMaxima { get; set; }
    
    // ========== OPTIONAL FIELDS (Advanced Flow) ==========
    
    /// <summary>
    /// Tax percentage (0-100). Optional.
    /// </summary>
    [JsonPropertyName("impuestoPorcentaje")]
    public decimal? ImpuestoPorcentaje { get; set; }
    
    /// <summary>
    /// Category ID. Optional.
    /// </summary>
    [JsonPropertyName("categoriaId")]
    public Guid? CategoriaId { get; set; }
    
    /// <summary>
    /// SKU code. If not provided, will be auto-generated.
    /// </summary>
    [JsonPropertyName("codigoSku")]
    public string? CodigoSku { get; set; }
    
    /// <summary>
    /// Product description. Optional.
    /// </summary>
    [JsonPropertyName("descripcion")]
    public string? Descripcion { get; set; }
    
    /// <summary>
    /// Product image URL from Supabase Storage. Optional.
    /// Frontend should upload image to Supabase and provide the public URL.
    /// </summary>
    [JsonPropertyName("imagenProductoUrl")]
    public string? ImagenProductoUrl { get; set; }
    
    /// <summary>
    /// Additional warehouses to add the product to.
    /// Each entry includes bodegaId and cantidadInicial.
    /// Cannot include the same bodega twice (including the main warehouse).
    /// </summary>
    [JsonPropertyName("bodegasAdicionales")]
    public List<ProductoBodegaDto>? BodegasAdicionales { get; set; }
    
    /// <summary>
    /// Predefined extra fields to link (by CampoExtraId).
    /// These CamposExtra must exist in the system before creating the product.
    /// </summary>
    [JsonPropertyName("camposExtra")]
    public List<ProductoCampoExtraDto>? CamposExtra { get; set; }
}

/// <summary>
/// DTO for adding a product to an additional warehouse.
/// </summary>
public class ProductoBodegaDto
{
    /// <summary>
    /// Warehouse ID
    /// </summary>
    [JsonPropertyName("bodegaId")]
    public Guid BodegaId { get; set; }
    
    /// <summary>
    /// Initial quantity in this warehouse
    /// </summary>
    [JsonPropertyName("cantidadInicial")]
    public int CantidadInicial { get; set; }
    
    /// <summary>
    /// Minimum quantity threshold (optional)
    /// </summary>
    [JsonPropertyName("cantidadMinima")]
    public int? CantidadMinima { get; set; }
    
    /// <summary>
    /// Maximum quantity threshold (optional)
    /// </summary>
    [JsonPropertyName("cantidadMaxima")]
    public int? CantidadMaxima { get; set; }
}

/// <summary>
/// DTO for linking a product to a predefined CampoExtra.
/// The CampoExtra must exist in the system.
/// </summary>
public class ProductoCampoExtraDto
{
    /// <summary>
    /// ID of the existing CampoExtra
    /// </summary>
    [JsonPropertyName("campoExtraId")]
    public Guid CampoExtraId { get; set; }
    
    /// <summary>
    /// Value for this specific product
    /// </summary>
    [JsonPropertyName("valor")]
    public string Valor { get; set; } = string.Empty;
}
