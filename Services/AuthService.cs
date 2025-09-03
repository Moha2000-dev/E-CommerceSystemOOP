using AutoMapper;
using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;


namespace E_CommerceSystem.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepo _authRepo;
        private readonly IMapper _mapper;
        private readonly IConfiguration _cfg;
        private readonly ICookieTokenWriter _cookies;
        private readonly ApplicationDbContext _db; // 👈 Inject DbContext لحفظ refresh

        private static readonly string[] AllowedRoles = new[] { "Admin", "Customer", "Manager" };

        public AuthService(IAuthRepo authRepo, IMapper mapper, IConfiguration cfg, ICookieTokenWriter cookies, ApplicationDbContext db)
        {
            _authRepo = authRepo;
            _mapper = mapper;
            _cfg = cfg;
            _cookies = cookies;
            _db = db;
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
            user.Role = Enum.Parse<User.UserRole>(role, true); // safe now
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
            dto.Token = CreateJwtToken(user); // 
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
                new Claim(ClaimTypes.Role, user.Role.ToString())
,
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwt["ExpiryInMinutes"] ?? "60")),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<LoginResponseDTO?> LoginWithCookiesAsync(string username, string password, HttpResponse res, string ip)
        {
            var user = await _authRepo.GetUserByUsernameAsync(username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password)) return null;

            var access = CreateJwtToken(user);
            var refresh = NewSecureToken();

            _db.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.UID,
                Token = refresh,
                ExpiresAt = DateTime.UtcNow.AddDays(_cfg.GetValue<int>("Jwt:RefreshTokenDays", 7)),
                CreatedByIp = ip
            });
            await _db.SaveChangesAsync();

            _cookies.WriteAccessCookie(res, access, _cfg.GetValue<int>("Jwt:AccessTokenMinutes", 15));
            _cookies.WriteRefreshCookie(res, refresh, _cfg.GetValue<int>("Jwt:RefreshTokenDays", 7));

            var dto = _mapper.Map<LoginResponseDTO>(user);
            dto.Token = access;
            return dto;
        }


        public async Task<LoginResponseDTO> RefreshAsync(HttpRequest req, HttpResponse res, string ip)
        {
            if (!req.Cookies.TryGetValue("refresh_token", out var presented))
                throw new Exception("Missing refresh token");

            var token = await _db.RefreshTokens.SingleOrDefaultAsync(t => t.Token == presented);
            if (token is null || !token.IsActive) throw new Exception("Invalid refresh token");

            // تدوير
            token.RevokedAt = DateTime.UtcNow;
            var newToken = new RefreshToken
            {
                UserId = token.UserId,
                Token = NewSecureToken(),
                ExpiresAt = DateTime.UtcNow.AddDays(_cfg.GetValue<int>("Jwt:RefreshTokenDays", 7)),
                CreatedByIp = ip,
                ReplacedByToken = token.Token
            };
            _db.RefreshTokens.Add(newToken);

            var user = await _authRepo.GetUserByIdAsync(token.UserId) ?? throw new Exception("User not found");
            var newAccess = CreateJwtToken(user);

            await _db.SaveChangesAsync();
            _cookies.WriteAccessCookie(res, newAccess, _cfg.GetValue<int>("Jwt:AccessTokenMinutes", 15));
            _cookies.WriteRefreshCookie(res, newToken.Token, _cfg.GetValue<int>("Jwt:RefreshTokenDays", 7));

            var dto = _mapper.Map<LoginResponseDTO>(user);
            dto.Token = newAccess;
            return dto;
        }



        public async Task LogoutAsync(HttpRequest req, HttpResponse res)
        {
            if (req.Cookies.TryGetValue("refresh_token", out var presented))
            {
                var t = await _db.RefreshTokens.SingleOrDefaultAsync(x => x.Token == presented);
                if (t != null) { t.RevokedAt = DateTime.UtcNow; await _db.SaveChangesAsync(); }
            }
            _cookies.ClearAuthCookies(res);
        }

        private static string NewSecureToken(int bytes = 64)
            => Convert.ToBase64String(RandomNumberGenerator.GetBytes(bytes));
    }



}

