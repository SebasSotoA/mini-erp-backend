namespace InventoryBack.Models.Domain
{
    public class ProductoCampoExtra
    {
        public Guid Id { get; set; }
        public Guid ProductoId { get; set; }
        public Guid CampoExtraId { get; set; }
        public string Valor { get; set; }
    }
}
