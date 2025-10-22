namespace InventoryBack.Domain.Entities
{
    public class FacturaCompra
    {
        public Guid Id { get; set; }
        public string NumeroFactura { get; set; }
        public Guid BodegaId { get; set; }
        public Guid ProveedorId { get; set; }
        public DateTime Fecha { get; set; }
        public string? Observaciones { get; set; }
        public string Estado { get; set; }
        public decimal Total { get; set; }
    }
}
