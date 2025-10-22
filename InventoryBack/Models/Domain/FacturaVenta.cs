namespace InventoryBack.Models.Domain
{
    public class FacturaVenta
    {
        public Guid Id { get; set; }
        public string NumeroFactura { get; set; }
        public Guid BodegaId { get; set; }
        public Guid VendedorId { get; set; }
        public DateTime Fecha { get; set; }
        public string FormaPago { get; set; }
        public int? PlazoPago { get; set; }
        public string MedioPago { get; set; }
        public string? Observaciones { get; set; }
        public string Estado { get; set; }
        public decimal Total { get; set; }
    }
}
