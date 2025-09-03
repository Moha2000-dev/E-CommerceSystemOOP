using AutoMapper;
using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;
using System.Threading.Tasks;

namespace E_CommerceSystem.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepo _authRepo;
        private readonly IMapper _mapper;

        public AuthService(IAuthRepo authRepo, IMapper mapper)
        {
            _authRepo = authRepo;
            _mapper = mapper;
        }

        public async Task<UserDTO> RegisterAsync(RegisterUserDTO dto)
        {
            var existingUser = await _authRepo.GetUserByUsernameAsync(dto.UName);
            if (existingUser != null)
                throw new Exception("Username already exists.");

            var user = _mapper.Map<User>(dto);
            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var createdUser = await _authRepo.AddUserAsync(user);

            return _mapper.Map<UserDTO>(createdUser);
        }

        public async Task<LoginResponseDTO?> LoginAsync(string username, string password)
        {
            var user = await _authRepo.GetUserByUsernameAsync(username);
            if (user == null) return null;

            if (!BCrypt.Net.BCrypt.Verify(password, user.Password)) return null;

            // AutoMapper maps User -> LoginResponseDTO
            var response = _mapper.Map<LoginResponseDTO>(user);
            return response;
        }
    }
}
