namespace InventoryBack.Domain.Entities
{
    public class Vendedor
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string Identificacion { get; set; }
        public string? Observaciones { get; set; }
        public string? Correo { get; set; }
    }
}
