using E_CommerceSystem.Models;
using System.Threading.Tasks;
namespace E_CommerceSystem.Services
{
    public interface IAuthService
    {
        Task<UserDTO> RegisterAsync(RegisterUserDTO dto);
        Task<LoginResponseDTO?> LoginAsync(string username, string password);
    }
}