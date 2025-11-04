namespace InventoryBack.Domain.Entities
{
    public class CampoExtra
    {
        public Guid Id { get; set; }
        public required string Nombre { get; set; }
        public required string TipoDato { get; set; }
        public bool EsRequerido { get; set; }
        public string? ValorPorDefecto { get; set; }
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
