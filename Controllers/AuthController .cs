using E_CommerceSystem.Models;
using E_CommerceSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace E_CommerceSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Register a new user (password is hashed with BCrypt).
        /// </summary>
        /// <param name="username">Unique username</param>
        /// <param name="password">Raw password (will be hashed)</param>
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDTO dto)
        {
            var user = await _authService.RegisterAsync(dto);
            return Ok(new { Message = "User registered successfully", user });
        }


        /// <summary>
        /// Login with username and password (BCrypt verification).
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromQuery] string username, [FromQuery] string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return BadRequest("Username and password are required.");

            var user = await _authService.LoginAsync(username, password);
            if (user == null)
                return Unauthorized("Invalid credentials");

            // For Day-1: return user info only
            return Ok(new
            {
                Message = "Login successful",
                user.UID,
                user.UName
            });
        }
    }
}
