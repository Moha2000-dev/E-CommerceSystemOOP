namespace E_CommerceSystem.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }                 // FK -> User.UID
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedByIp { get; set; } = string.Empty;
        public DateTime? RevokedAt { get; set; }
        public string? ReplacedByToken { get; set; }    // if rotated
        public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;
    }
}
