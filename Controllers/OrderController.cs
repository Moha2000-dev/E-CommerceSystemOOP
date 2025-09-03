using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using E_CommerceSystem.Models;
using E_CommerceSystem.Services;
using System.IdentityModel.Tokens.Jwt;

namespace E_CommerceSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IOrderSummaryService _summaryService;
        public OrderController(IOrderService orderService, IOrderSummaryService summaryService)
        {
            _orderService = orderService;
            _summaryService = summaryService;
        }
        [HttpPost("PlaceOrder")]
        public IActionResult PlaceOrder([FromBody] List<OrderItemDTO> items)
        {
            if (items is null || items.Count == 0) return BadRequest("Order items cannot be empty.");

            var uid = GetUserId();                    // <- from claims
            _orderService.PlaceOrder(items, uid);

            return Ok("Order placed successfully.");
        }

        [HttpGet("GetAllOrders")]
        public IActionResult GetAllOrders()
        {
            var uid = GetUserId();
            var result = _orderService.GetAllOrders(uid);   // should return DTOs
            return Ok(result);
        }

        [HttpGet("GetOrderById/{orderId:int}")]
        public IActionResult GetOrderById(int orderId)
        {
            var uid = GetUserId();
            var result = _orderService.GetOrderById(orderId, uid); // DTO
            return result is not null ? Ok(result) : NotFound();
        }

        private int GetUserId()
        {
            // prefer NameIdentifier; fall back to "sub"
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                     User.FindFirst("sub")?.Value;

            if (string.IsNullOrWhiteSpace(id) || !int.TryParse(id, out var uid))
                throw new UnauthorizedAccessException("User id not found in token.");
            return uid;
        }
        [HttpPatch("{orderId:int}/status")]
        public IActionResult UpdateStatus(int orderId, [FromQuery] OrderStatus status)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var userId = GetUserIdFromToken(token);
                var uid = int.Parse(userId);

                var ok = _orderService.UpdateStatus(orderId, uid, status);
                return ok ? Ok("Status updated.") : NotFound("Order not found, not owned, or cannot update.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating status. {ex.Message}");
            }
        }

        [HttpPost("{orderId:int}/cancel")]
        public IActionResult Cancel(int orderId)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var userId = GetUserIdFromToken(token);
                var uid = int.Parse(userId);

                var ok = _orderService.Cancel(orderId, uid);
                return ok ? Ok("Order cancelled and stock restored.") : BadRequest("Cannot cancel this order.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error cancelling order. {ex.Message}");
            }
        }

        private string? GetUserIdFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            if (handler.CanReadToken(token))
            {
                var jwtToken = handler.ReadJwtToken(token);
                var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub");
                return subClaim?.Value;
            }
            throw new UnauthorizedAccessException("Invalid or unreadable token.");
        }
        [HttpGet("summary")]
        [Authorize(Roles = "Admin,Manager")] // optional: restrict to roles later
        public async Task<IActionResult> Summary(DateTime from, DateTime to)
        {
            var result = await _summaryService.GetSummaryAsync(from, to);
            return Ok(result);
        }


    }
}