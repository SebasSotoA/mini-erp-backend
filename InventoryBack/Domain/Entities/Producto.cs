namespace InventoryBack.Domain.Entities
{
    public class Producto
    {
        public Guid Id { get; set; }
        public required string Nombre { get; set; }
        public required string UnidadMedida { get; set; }
        public decimal PrecioBase { get; set; }
        public decimal? ImpuestoPorcentaje { get; set; }
        public decimal CostoInicial { get; set; }
        public Guid? CategoriaId { get; set; }
        public string? CodigoSku { get; set; }
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string? ImagenProductoUrl { get; set; }
        
        /// <summary>
        /// ID of the main warehouse where this product is primarily stored
        /// </summary>
        public Guid BodegaPrincipalId { get; set; }
    }
}
