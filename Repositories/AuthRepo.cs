using E_CommerceSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceSystem.Repositories
{
    public class AuthRepo : IAuthRepo
    {
        private readonly ApplicationDbContext _db;
        public AuthRepo(ApplicationDbContext db) => _db = db;

        public Task<User?> GetUserByUsernameAsync(string username) =>
            _db.Users.FirstOrDefaultAsync(u => u.UName == username);

        public Task<User?> GetUserByEmailAsync(string email) =>
            _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        public async Task<User> AddUserAsync(User user)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }
        public Task<User?> GetUserByIdAsync(int uid)
       => _db.Users.SingleOrDefaultAsync(u => u.UID == uid);
        public async Task AddRefreshTokenAsync(RefreshToken token)
        {
            _db.RefreshTokens.Add(token);
            await _db.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
            => await _db.RefreshTokens.SingleOrDefaultAsync(r => r.Token == token);

        public async Task RevokeRefreshTokenAsync(string token)
        {
            var t = await _db.RefreshTokens.SingleOrDefaultAsync(r => r.Token == token);
            if (t != null)
            {
                t.RevokedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        }
    }
}
