using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using E_CommerceSystem.Models;
using E_CommerceSystem.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPdfDoc = QuestPDF.Fluent.Document;



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

        [HttpGet("{orderId:int}/invoice-pdf")]
        public IActionResult InvoicePdf(int orderId)
        {
            var uidStr = User.FindFirst("sub")?.Value
                         ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(uidStr, out var uid))
                return Unauthorized("User id missing.");

            var items = _orderService.GetOrderById(orderId, uid).ToList();
            if (!items.Any()) return NotFound("Order not found or not owned by user.");

            var orderDate = items.First().OrderDate;
            var total = items.Sum(i => i.TotalAmount);

            var pdf = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(36);
                    page.Size(PageSizes.A4);
                    page.Header().Text($"Invoice #{orderId}")
                        .SemiBold().FontSize(20).FontColor(Colors.Black);

                    page.Content().Column(col =>
                    {
                        col.Spacing(10);

                        col.Item().Text($"Date: {orderDate:yyyy-MM-dd}").FontSize(10);

                        col.Item().Table(table =>
                        {
                            // define helpers ONCE for the whole table scope
                            Func<IContainer, IContainer> CellHeader = c =>
                                c.DefaultTextStyle(x => x.SemiBold())
                                 .Padding(6).Background(Colors.Grey.Lighten3)
                                 .Border(1).BorderColor(Colors.Grey.Lighten2);

                            Func<IContainer, IContainer> CellBody = c =>
                                c.Padding(6).Border(1).BorderColor(Colors.Grey.Lighten3);

                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(6); // Product
                                columns.RelativeColumn(2); // Qty
                                columns.RelativeColumn(3); // Line total
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellHeader).Text("Product");
                                header.Cell().Element(CellHeader).AlignCenter().Text("Qty");
                                header.Cell().Element(CellHeader).AlignRight().Text("Line Total");
                            });

                            foreach (var it in items)
                            {
                                table.Cell().Element(CellBody).Text(it.ProductName);
                                table.Cell().Element(CellBody).AlignCenter().Text(it.Quantity.ToString());
                                table.Cell().Element(CellBody).AlignRight().Text($"{it.TotalAmount:C}");
                            }

                            // footer total row
                            table.Cell().ColumnSpan(2).Element(CellBody).AlignRight().Text("Total").SemiBold();
                            table.Cell().Element(CellBody).AlignRight().Text($"{total:C}").SemiBold();
                        });


                        col.Item().Text("Thanks for your purchase!")
                                  .FontSize(10).FontColor(Colors.Grey.Darken1);
                    });
                });
            }).GeneratePdf();

            return File(pdf, "application/pdf", $"invoice_{orderId}.pdf");
        }



    }
}