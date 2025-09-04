using E_CommerceSystem.DTOs;
using E_CommerceSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_CommerceSystem.Controllers
{
    [ApiController]
    [Route("api/admin/reports")]
    [Authorize(Roles = "Admin,Manager")]
    public class AdminReportsController : ControllerBase
    {
        private readonly IAdminReportService _svc;
        public AdminReportsController(IAdminReportService svc) => _svc = svc;

        [HttpGet("best-selling")]
        public async Task<ActionResult<IReadOnlyList<BestSellingProductDto>>> BestSelling(
            [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int top = 10)
            => Ok(await _svc.BestSellingAsync(from, to, top));

        [HttpGet("revenue/daily")]
        public async Task<ActionResult<IReadOnlyList<RevenuePointDto>>> RevenueDaily(
            [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var start = from ?? DateTime.UtcNow.AddDays(-30).Date;
            var end = to ?? DateTime.UtcNow.Date.AddDays(1);
            return Ok(await _svc.RevenueDailyAsync(start, end));
        }

        [HttpGet("revenue/monthly")]
        public async Task<ActionResult<IReadOnlyList<RevenueMonthDto>>> RevenueMonthly([FromQuery] int? year)
            => Ok(await _svc.RevenueMonthlyAsync(year ?? DateTime.UtcNow.Year));

        [HttpGet("top-rated")]
        public async Task<ActionResult<IReadOnlyList<TopRatedProductDto>>> TopRated(
            [FromQuery] int minReviews = 1, [FromQuery] int top = 10)
            => Ok(await _svc.TopRatedAsync(minReviews, top));

        [HttpGet("most-active-customers")]
        public async Task<ActionResult<IReadOnlyList<ActiveCustomerDto>>> MostActive(
            [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int top = 10)
            => Ok(await _svc.MostActiveAsync(from, to, top));
    }
}
