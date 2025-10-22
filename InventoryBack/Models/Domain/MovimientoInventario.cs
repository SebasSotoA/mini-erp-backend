namespace InventoryBack.Models.Domain
{
    public class MovimientoInventario
    {
        public Guid Id { get; set; }
        public Guid ProductoId { get; set; }
        public Guid BodegaId { get; set; }
        public DateTime Fecha { get; set; }
        public string TipoMovimiento { get; set; }
        public int Cantidad { get; set; }
        public decimal? CostoUnitario { get; set; }
        public decimal? PrecioUnitario { get; set; }
        public string? Observacion { get; set; }
        public Guid? FacturaId { get; set; }
    }
}
