using AutoMapper;
using AutoMapper.QueryableExtensions;
using E_CommerceSystem.DTOs;
using E_CommerceSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceSystem.Repositories
{
    public class AdminReportRepo : IAdminReportRepo
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

       
        private static readonly OrderStatus[] RevenueStatuses = new[] { OrderStatus.Paid, OrderStatus.Delivered };

        public AdminReportRepo(ApplicationDbContext db, IMapper mapper)
        { _db = db; _mapper = mapper; }

        public async Task<IReadOnlyList<BestSellingProductDto>> GetBestSellingAsync(DateTime? from, DateTime? to, int top)
        {
            var q = _db.OrderProducts
                       .Include(op => op.Order)
                       .Include(op => op.product)
                       .Where(op => RevenueStatuses.Contains(op.Order.Status));

            if (from.HasValue) q = q.Where(op => op.Order.OrderDate >= from.Value);
            if (to.HasValue) q = q.Where(op => op.Order.OrderDate < to.Value);

            return await q
                .GroupBy(op => new { op.PID, op.product.ProductName, op.product.ImageUrl })
                .Select(g => new BestSellingProductDto
                {
                    ProductId = g.Key.PID,
                    ProductName = g.Key.ProductName,
                    ImageUrl = g.Key.ImageUrl,
                    TotalQuantity = g.Sum(x => x.Quantity),
                    TotalRevenue = g.Sum(x => x.Quantity * x.Order.TotalAmount /
                                              (x.Order.OrderProducts.Sum(p => p.Quantity) == 0 ? 1 : x.Order.OrderProducts.Sum(p => p.Quantity)))
              
                })
                .OrderByDescending(x => x.TotalQuantity)
                .ThenByDescending(x => x.TotalRevenue)
                .Take(top)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<RevenuePointDto>> GetDailyRevenueAsync(DateTime from, DateTime to)
        {
            var q = _db.Orders
                       .Where(o => RevenueStatuses.Contains(o.Status)
                                && o.OrderDate >= from && o.OrderDate < to);

            return await q
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new RevenuePointDto { Date = g.Key, Revenue = g.Sum(x => x.TotalAmount) })
                .OrderBy(x => x.Date)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<RevenueMonthDto>> GetMonthlyRevenueAsync(int year)
        {
            var q = _db.Orders
                       .Where(o => RevenueStatuses.Contains(o.Status) && o.OrderDate.Year == year);

            return await q
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new RevenueMonthDto { Year = g.Key.Year, Month = g.Key.Month, Revenue = g.Sum(x => x.TotalAmount) })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<TopRatedProductDto>> GetTopRatedProductsAsync(int minReviews, int top)
        {
            return await _db.Products
                .Where(p => p.Reviews.Count >= minReviews)
                .OrderByDescending(p => p.Reviews.Average(r => (double?)r.Rating) ?? 0)
                .ThenByDescending(p => p.Reviews.Count)
                .Take(top)
                .ProjectTo<TopRatedProductDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<ActiveCustomerDto>> GetMostActiveCustomersAsync(DateTime? from, DateTime? to, int top)
        {
            var q = _db.Orders
                       .Include(o => o.user)
                       .Where(o => RevenueStatuses.Contains(o.Status));

            if (from.HasValue) q = q.Where(o => o.OrderDate >= from.Value);
            if (to.HasValue) q = q.Where(o => o.OrderDate < to.Value);

            return await q
                .GroupBy(o => new { o.UID, o.user.UName })
                .Select(g => new ActiveCustomerDto
                {
                    UserId = g.Key.UID,
                    UserName = g.Key.UName,
                    OrdersCount = g.Count(),
                    TotalSpent = g.Sum(x => x.TotalAmount)
                })
                .OrderByDescending(x => x.OrdersCount)
                .ThenByDescending(x => x.TotalSpent)
                .Take(top)
                .ToListAsync();
        }
    }
}
