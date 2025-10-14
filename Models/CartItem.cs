namespace Proyecto_v1.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public DateTime AddedDate { get; set; }

        // Navegación
        public Cart Cart { get; set; }
        public Product Product { get; set; }

        // Propiedad calculada
        public decimal TotalPrice => Quantity * UnitPrice;
    }
}