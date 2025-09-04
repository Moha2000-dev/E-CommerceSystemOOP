using System.Threading.Tasks;
using E_CommerceSystem.Models;

namespace E_CommerceSystem.Repositories
{
    public interface IAuthRepo
    {
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User> AddUserAsync(User user);

        Task<User?> GetUserByIdAsync(int uid);
        Task AddRefreshTokenAsync(RefreshToken token);
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task RevokeRefreshTokenAsync(string token);
    }
}
