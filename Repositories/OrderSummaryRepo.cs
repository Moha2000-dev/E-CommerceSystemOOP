using Microsoft.EntityFrameworkCore;

namespace E_CommerceSystem.Repositories
{
    public class OrderSummaryRepo : IOrderSummaryRepo
    {
        private readonly ApplicationDbContext _db;

        public OrderSummaryRepo(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<int> GetTotalOrdersAsync(DateTime from, DateTime to)
        {
            return await _db.Orders
                .Where(o => o.OrderDate >= from && o.OrderDate < to)
                .CountAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime from, DateTime to)
        {
            return await _db.Orders
                .Where(o => o.OrderDate >= from && o.OrderDate < to)
                .SumAsync(o => o.TotalAmount);
        }

        public async Task<List<object>> GetTopCustomersAsync(DateTime from, DateTime to)
        {
            return await _db.Orders
                .Include(o => o.user)
                .Where(o => o.OrderDate >= from && o.OrderDate < to)
                .GroupBy(o => new { o.UID, o.user.UName })
                .Select(g => new
                {
                    UserId = g.Key.UID,
                    Username = g.Key.UName,
                    OrdersCount = g.Count(),
                    TotalSpent = g.Sum(o => o.TotalAmount)
                })
                .OrderByDescending(x => x.OrdersCount)
                .Take(5)
                .Cast<object>() // keep it generic like in your service
                .ToListAsync();
        }

        // Optional if you want top products too
        /*
        public async Task<List<object>> GetTopProductsAsync(DateTime from, DateTime to)
        {
            return await _db.OrderProducts
                .Include(op => op.product)
                .Where(op => op.Order.OrderDate >= from && op.Order.OrderDate < to)
                .GroupBy(op => new { op.PID, op.product.ProductName })
                .Select(g => new
                {
                    ProductId = g.Key.PID,
                    Name = g.Key.ProductName,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.Quantity)
                .Take(5)
                .Cast<object>()
                .ToListAsync();
        }
        */
    }
}
