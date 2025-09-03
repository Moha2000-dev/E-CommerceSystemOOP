using E_CommerceSystem.Models;

namespace E_CommerceSystem.Repositories
{
    public interface IAuthRepo
    {
        Task<User> AddUserAsync(User user);
        Task<User?> GetUserByUsernameAsync(string username);
    }
}