namespace E_CommerceSystem.Models
{
    // Category class to represent product categories
    public class Category
    {
        public int CategoryId { get; set; }  // Primary key for Category
        public string Name { get; set; } = string.Empty; // Category name
        public string? Description { get; set; } // Optional description

        // Navigation property for related products
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
