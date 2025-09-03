using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace E_CommerceSystem.Models
{
    public class User
    {

        public enum UserRole { Customer = 0, Manager = 1, Admin = 2 }

        [Key]
        public int UID { get; set; }

        [Required]
        public string UName { get; set; }

        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        ErrorMessage = "Invalid email format.(e.g 'example@gmail.com')")]
        public string Email { get; set; }

        [JsonIgnore]
        [Required]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Password must be at least 8 characters long, contain at least one uppercase letter," +
            " one lowercase letter, one digit, and one special character.")]
        public string Password { get; set; }

        [Required]
        public string Phone {  get; set; }

        [Required]
        public UserRole Role { get; set; } = UserRole.Customer;

        public DateTime CreatedAt { get; set; }

        [JsonIgnore]
        public virtual ICollection<Order> Orders { get; set; }

        [JsonIgnore]
        public virtual ICollection<Review> Reviews { get; set; }

    }
}
