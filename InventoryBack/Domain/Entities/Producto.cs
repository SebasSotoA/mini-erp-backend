﻿namespace InventoryBack.Domain.Entities
{
    public class Producto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string UnidadMedida { get; set; }
        public decimal PrecioBase { get; set; }
        public decimal? ImpuestoPorcentaje { get; set; }
        public decimal CostoInicial { get; set; }
        public Guid? CategoriaId { get; set; }
        public string? CodigoSku { get; set; }
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string? ImagenProductoUrl { get; set; }
    }
}
