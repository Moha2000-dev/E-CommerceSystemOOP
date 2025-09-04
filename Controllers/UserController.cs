using AutoMapper;
using E_CommerceSystem.Models;
using E_CommerceSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public IActionResult Register([FromBody] UserDTO inputUser)
        {
            try
            {
                if (inputUser == null)
                    return BadRequest("User data is required.");

                // DTO -> Entity via AutoMapper
                var user = _mapper.Map<User>(inputUser);
                user.CreatedAt = DateTime.UtcNow; // set here (profile ignores it)

                // (Optional) hash password here or in service
                 //user.Password = _passwordHasher.Hash(inputUser.Password);

                _userService.AddUser(user);

                // Entity -> DTO (but never include password)
                var result = _mapper.Map<UserDTO>(user);
                // ensure we don’t expose it even if someone later changes the profile
                result.Password = null;

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while adding the user. {ex.Message}");
            }
        }

        // keeping your GET login shape (though POST is recommended)
        [AllowAnonymous]
        [HttpGet("Login")]
        public IActionResult Login([FromQuery] string email, [FromQuery] string password)
        {
            try
            {
                var user = _userService.GetUSer(email, password);
                if (user == null) return Unauthorized("Invalid credentials.");

                string token = GenerateJwtToken(user.UID.ToString(), user.UName, user.Role.ToString());
                return Ok(token);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while login. {ex.Message}");
            }
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
