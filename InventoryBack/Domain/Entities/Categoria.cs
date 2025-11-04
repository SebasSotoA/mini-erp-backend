namespace InventoryBack.Domain.Entities
{
    public class Categoria
    {
        public Guid Id { get; set; }
        public required string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string? ImagenCategoriaUrl { get; set; }
    }
}
