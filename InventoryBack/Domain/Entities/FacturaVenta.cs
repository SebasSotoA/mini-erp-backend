namespace InventoryBack.Domain.Entities
{
    public class FacturaVenta
    {
        public Guid Id { get; set; }
        public required string NumeroFactura { get; set; }
        public Guid BodegaId { get; set; }
        public Guid VendedorId { get; set; }
        public DateTime Fecha { get; set; }
        public required string FormaPago { get; set; }
        public int? PlazoPago { get; set; }
        public required string MedioPago { get; set; }
        public string? Observaciones { get; set; }
        public required string Estado { get; set; }
        public decimal Total { get; set; }
    }
}
