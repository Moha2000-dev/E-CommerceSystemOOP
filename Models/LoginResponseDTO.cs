namespace E_CommerceSystem.Models
{
    public class LoginResponseDTO
    {
        public string Message { get; set; }
        public int UID { get; set; }
        public string UName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }

        // Day-2 placeholder (tokens will be filled later)
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public string Token { get; set; } = null!;
    }
}
