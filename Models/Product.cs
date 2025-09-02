using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace E_CommerceSystem.Models
{
    public class Product
    {
        [Key]
        public int PID { get; set; }

        [Required]
        public string ProductName { get; set; }

        public string Description { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Stock {  get; set; }

        public decimal OverallRating { get; set; }

        [JsonIgnore]
        public virtual ICollection<OrderProducts> OrderProducts { get;set; }

        [JsonIgnore]
        public virtual ICollection<Review> Reviews { get; set; }

        public int CategoryId { get; set; }        // FK to Category
        public Category? Category { get; set; }    // Navigation

        public int SupplierId { get; set; }        // FK to Supplier
        public Supplier? Supplier { get; set; }    // Navigation
        public string? ImageUrl { get; set; }

    }
}
