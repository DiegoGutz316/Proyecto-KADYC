namespace Proyecto_v1.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Size { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public double Price { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }

        // Clave foránea explícita
        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
