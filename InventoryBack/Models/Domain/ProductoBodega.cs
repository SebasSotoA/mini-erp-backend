namespace InventoryBack.Models.Domain
{
    public class ProductoBodega
    {
        public Guid Id { get; set; }
        public Guid ProductoId { get; set; }
        public Guid BodegaId { get; set; }
        public int CantidadInicial { get; set; }
        public int? CantidadMinima { get; set; }
        public int? CantidadMaxima { get; set; }
    }
}
