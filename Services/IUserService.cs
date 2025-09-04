using E_CommerceSystem.Models;

namespace E_CommerceSystem.Services
{
    public interface IUserService
    {
        void AddUser(User user);
        void DeleteUser(int uid);
        IEnumerable<User> GetAllUsers();
        User GetUser(string email, string password);
        User GetUserById(int uid);
        void UpdateUser(User user);
        bool ExistsByEmail(string email);
        User GetUserByEmail(string email);

        User GetByEmailOrUName(string key);
    }
}