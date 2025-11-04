namespace InventoryBack.Domain.Entities
{
    public class Vendedor
    {
        public Guid Id { get; set; }
        public required string Nombre { get; set; }
        public required string Identificacion { get; set; }
        public string? Observaciones { get; set; }
        public string? Correo { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
