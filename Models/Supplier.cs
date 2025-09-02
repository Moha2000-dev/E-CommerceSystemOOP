namespace E_CommerceSystem.Models
{

    // Supplier class to represent product suppliers
    public class Supplier
    {
        // Primary key for Supplier
        public int SupplierId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string? Phone { get; set; }

        // Navigation property for related products supplied by this supplier 
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
