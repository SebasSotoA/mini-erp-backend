namespace InventoryBack.Models.Domain
{
    public class CampoExtra
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string TipoDato { get; set; }
        public bool EsRequerido { get; set; }
        public string? ValorPorDefecto { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
