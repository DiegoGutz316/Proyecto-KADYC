namespace Proyecto_v1.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public string UserId { get; set; } // ID del usuario autenticado
        public int? CustomerId { get; set; } // ID del customer asociado
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();

        // Navegación
        public Customer? Customer { get; set; }

        // Propiedades calculadas
        public decimal TotalAmount => CartItems?.Sum(item => item.TotalPrice) ?? 0;
        public int TotalItems => CartItems?.Sum(item => item.Quantity) ?? 0;
    }
}