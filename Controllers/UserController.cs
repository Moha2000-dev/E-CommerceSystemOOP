using AutoMapper;
using E_CommerceSystem.Models;
using E_CommerceSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace E_CommerceSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public UserController(IUserService userService, IConfiguration configuration, IMapper mapper)
        {
            _userService = userService;
            _configuration = configuration;
            _mapper = mapper;
        }
        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register([FromBody] RegisterUserDTO dto)
        {
            if (dto == null)
                return BadRequest("User data is required.");

            // Hash only once
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                UName = dto.UName,
                Email = dto.Email?.Trim().ToLower(),
                Password = hashedPassword,   // ✅ store the hashed string
                Phone = dto.Phone,
                Role = Enum.Parse<User.UserRole>(dto.Role, true),
                CreatedAt = DateTime.UtcNow
            };

            _userService.AddUser(user);

            return Ok(new { uname = user.UName, email = user.Email, phone = user.Phone, role = user.Role.ToString() });
        }

        // keeping your GET login shape (though POST is recommended)
        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginRequestDTO dto)
        {
            if (dto == null) return BadRequest("Body is required.");

            var key = dto.Email?.Trim();                 // keep property name "email" for Swagger
            var email = key?.ToLower();

            // 1) Lookup by email OR username (helps catch wrong field)
            var user = _userService.GetByEmailOrUName(key);
            if (user == null) return Unauthorized("No user with this email/username.");

            // 2) Check bcrypt verify
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
                return Unauthorized("Password mismatch.");

            var token = GenerateJwtToken(user.UID.ToString(), user.UName, user.Role.ToString());
            return Ok(new { token });
        }




        [HttpGet("GetUserById/{userId:int}")]
        public IActionResult GetUserById(int userId)
        {
            try
            {
                var user = _userService.GetUserById(userId);
                if (user == null) return NotFound();

                // Entity -> DTO (no password)
                var dto = _mapper.Map<UserDTO>(user);
                dto.Password = null;

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving user. {ex.Message}");
            }
        }

        [NonAction]
        public string GenerateJwtToken(string userId, string username, string role)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var secretKey = jwtSettings["SecretKey"];

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Name, username),
                // you currently store role in "unique_name"; consider adding ClaimTypes.Role too:
                new Claim(JwtRegisteredClaimNames.UniqueName, role),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiryInMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
