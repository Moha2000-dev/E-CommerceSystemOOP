using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using E_CommerceSystem.Models;
using E_CommerceSystem.Services;
using System.IdentityModel.Tokens.Jwt;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace E_CommerceSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IOrderSummaryService _summaryService;
        private readonly ILogger<OrderController> _logger;
        public OrderController(IOrderService orderService, IOrderSummaryService summaryService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _summaryService = summaryService;
            _logger = logger;
        }

        [HttpPost("PlaceOrder")]
        public IActionResult PlaceOrder([FromBody] List<OrderItemDTO> items)
        {
            if (items is null || items.Count == 0)
                return BadRequest("Order items cannot be empty.");

            var uid = GetUserId();
            _orderService.PlaceOrder(items, uid);
            _logger.LogInformation("User {UserId} placing order with {ItemCount} items", uid, items.Count);
            _logger.LogInformation("Order placed successfully by User {UserId}", uid);
            return Ok("Order placed successfully.");
        }

        [HttpGet("GetAllOrders")]
        public IActionResult GetAllOrders()
        {
            var uid = GetUserId();
            var result = _orderService.GetAllOrders(uid);
            return Ok(result);
        }

        [HttpGet("GetOrderById/{orderId:int}")]
        public IActionResult GetOrderById(int orderId)
        {
            var uid = GetUserId();
            var result = _orderService.GetOrderById(orderId, uid);
            return result is not null ? Ok(result) : NotFound();
        }

        [HttpPatch("{orderId:int}/status")]
        public IActionResult UpdateStatus(int orderId, [FromQuery] OrderStatus status)
        {
            var uid = GetUserId();
            var ok = _orderService.UpdateStatus(orderId, uid, status);
            return ok
                ? Ok("Status updated.")
                : NotFound("Order not found, not owned, or cannot update.");
        }

        [HttpPost("{orderId:int}/cancel")]
        public IActionResult Cancel(int orderId)
        {
            var uid = GetUserId();
            var ok = _orderService.Cancel(orderId, uid);
            return ok
                ? Ok("Order cancelled and stock restored.")
                : BadRequest("Cannot cancel this order.");
        }

        [HttpGet("summary")]
        [Authorize(Roles = "admin,Manager")]
        public async Task<IActionResult> Summary(DateTime from, DateTime to)
        {
            var result = await _summaryService.GetSummaryAsync(from, to);
            return Ok(result);
        }

        [HttpGet("{orderId:int}/invoice-pdf")]
        public IActionResult InvoicePdf(int orderId)
        {
            var uid = GetUserId();
            var items = _orderService.GetOrderById(orderId, uid).ToList();
            if (!items.Any())
                return NotFound("Order not found or not owned by user.");

            var orderDate = items.First().OrderDate;
            var total = items.Sum(i => i.TotalAmount);

            var pdf = Document.Create(container =>
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

        // 🔹 Helper: get logged-in user id from claims
        private int GetUserId()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                     User.FindFirst("sub")?.Value;

            if (string.IsNullOrWhiteSpace(id) || !int.TryParse(id, out var uid))
                throw new UnauthorizedAccessException("User id not found in token.");

            return uid;
        }
    }
}
