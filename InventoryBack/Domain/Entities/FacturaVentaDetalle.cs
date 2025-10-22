namespace InventoryBack.Domain.Entities
{
    public class FacturaVentaDetalle
    {
        public Guid Id { get; set; }
        public Guid FacturaVentaId { get; set; }
        public Guid ProductoId { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal? Descuento { get; set; }
        public decimal? Impuesto { get; set; }
        public int Cantidad { get; set; }
        public decimal TotalLinea { get; set; }
    }
}
