namespace E_CommerceSystem.Models
{
    public class RegisterUserDTO
    {
        public string UName { get; set; }
        public string Password { get; set; }   // plain password, hash in service
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; } = "Customer"; // default
    }
}
