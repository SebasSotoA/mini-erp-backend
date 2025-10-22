namespace InventoryBack.Models.Domain
{
    public class FacturaCompraDetalle
    {
        public Guid Id { get; set; }
        public Guid FacturaCompraId { get; set; }
        public Guid ProductoId { get; set; }
        public decimal CostoUnitario { get; set; }
        public decimal? Descuento { get; set; }
        public decimal? Impuesto { get; set; }
        public int Cantidad { get; set; }
        public decimal TotalLinea { get; set; }
    }
}
