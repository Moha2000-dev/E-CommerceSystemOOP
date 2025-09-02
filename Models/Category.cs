namespace E_CommerceSystem.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        // nav
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
