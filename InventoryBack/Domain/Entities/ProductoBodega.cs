namespace InventoryBack.Domain.Entities
{
    public class ProductoBodega
    {
        public Guid Id { get; set; }
        public Guid ProductoId { get; set; }
        public Guid BodegaId { get; set; }
        
        /// <summary>
        /// Stock actual del producto en esta bodega.
        /// Se actualiza automáticamente con cada movimiento de inventario.
        /// </summary>
        public int StockActual { get; set; }
        
        public int? CantidadMinima { get; set; }
        public int? CantidadMaxima { get; set; }
    }
}
