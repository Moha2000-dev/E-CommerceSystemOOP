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

        // ONE constructor only
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDTO dto)
        {
            if (dto is null) return BadRequest("Body is required.");
            var user = await _authService.RegisterAsync(dto);
            return Ok(new { message = "User registered successfully", user });
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserDTO dto)
        {
            if (dto is null) return BadRequest("Body is required.");
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var resp = await _authService.LoginWithCookiesAsync(dto.UName, dto.Password, Response, ip);
            if (resp is null) return Unauthorized("Invalid credentials.");
            return Ok(resp);
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var resp = await _authService.RefreshAsync(Request, Response, ip);
            return Ok(resp);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync(Request, Response);
            return Ok(new { message = "Logged out" });
        }
    }
}

