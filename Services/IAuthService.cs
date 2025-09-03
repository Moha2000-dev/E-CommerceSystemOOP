using System.Threading.Tasks;
using E_CommerceSystem.Models;

namespace E_CommerceSystem.Services
{
    public interface IAuthService
    {
        Task<UserDTO> RegisterAsync(RegisterUserDTO dto);
        Task<LoginResponseDTO?> LoginAsync(string username, string password);
        string CreateJwtToken(User user); // new: issue JWT with role claim
    }
}
