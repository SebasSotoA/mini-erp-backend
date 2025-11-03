namespace InventoryBack.Domain.Entities
{
    /// <summary>
    /// Represents an inventory movement record for audit and traceability.
    /// </summary>
    public class MovimientoInventario
    {
        public Guid Id { get; set; }
        public Guid ProductoId { get; set; }
        public Guid BodegaId { get; set; }
        public DateTime Fecha { get; set; }
        public string TipoMovimiento { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal? CostoUnitario { get; set; }
        public decimal? PrecioUnitario { get; set; }
        public string? Observacion { get; set; }
        
        // Foreign Keys
        public Guid? FacturaVentaId { get; set; }
        public Guid? FacturaCompraId { get; set; }

        // Navigation Properties (for EF Core eager loading)
        public Producto? Producto { get; set; }
        public Bodega? Bodega { get; set; }
        public FacturaVenta? FacturaVenta { get; set; }
        public FacturaCompra? FacturaCompra { get; set; }
    }
}
