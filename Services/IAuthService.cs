using System.Threading.Tasks;
using E_CommerceSystem.Models;

namespace E_CommerceSystem.Services
{
    public interface IAuthService
    {
        Task<UserDTO> RegisterAsync(RegisterUserDTO dto);
        Task<LoginResponseDTO?> LoginAsync(string username, string password);
        string CreateJwtToken(User user); // new: issue JWT with role claim

        Task<LoginResponseDTO?> LoginWithCookiesAsync(string username, string password, HttpResponse res, string ip);
        Task<LoginResponseDTO> RefreshAsync(HttpRequest req, HttpResponse res, string ip);
        Task LogoutAsync(HttpRequest req, HttpResponse res);
    }
}
