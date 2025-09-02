using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using E_CommerceSystem.Models;
using E_CommerceSystem.Services;

namespace E_CommerceSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService) => _orderService = orderService;

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
    }
}