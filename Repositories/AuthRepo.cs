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
    }
}
