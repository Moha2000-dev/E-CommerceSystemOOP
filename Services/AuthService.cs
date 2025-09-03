using AutoMapper;
using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace E_CommerceSystem.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepo _authRepo;
        private readonly IMapper _mapper;
        private readonly IConfiguration _cfg;

        private static readonly string[] AllowedRoles = new[] { "Admin", "Customer", "Manager" };

        public AuthService(IAuthRepo authRepo, IMapper mapper, IConfiguration cfg)
        {
            _authRepo = authRepo;
            _mapper = mapper;
            _cfg = cfg;
        }

        public async Task<UserDTO> RegisterAsync(RegisterUserDTO dto)
        {
            if (await _authRepo.GetUserByUsernameAsync(dto.UName) is not null)
                throw new Exception("Username already exists.");
            if (await _authRepo.GetUserByEmailAsync(dto.Email) is not null)
                throw new Exception("Email already exists.");

            var role = AllowedRoles.FirstOrDefault(r => string.Equals(r, dto.Role, StringComparison.OrdinalIgnoreCase))
                       ?? "Customer";

            var user = _mapper.Map<User>(dto);
            user.Role = role;
            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var created = await _authRepo.AddUserAsync(user);
            return _mapper.Map<UserDTO>(created);
        }

        public async Task<LoginResponseDTO?> LoginAsync(string username, string password)
        {
            var user = await _authRepo.GetUserByUsernameAsync(username);
            if (user == null) return null;
            if (!BCrypt.Net.BCrypt.Verify(password, user.Password)) return null;

            var dto = _mapper.Map<LoginResponseDTO>(user);
            dto.Token = CreateJwtToken(user);
            return dto;
        }

        public string CreateJwtToken(User user)
        {
            var jwt = _cfg.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UID.ToString()),
                new Claim(ClaimTypes.Name, user.UName),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwt["ExpiryInMinutes"] ?? "60")),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
