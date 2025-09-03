using Microsoft.EntityFrameworkCore;

namespace E_CommerceSystem.Services
{
    public class OrderSummaryService : IOrderSummaryService
    {
        private readonly ApplicationDbContext _db;

        public OrderSummaryService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<object> GetSummaryAsync(DateTime from, DateTime to)
        {
            // Filter orders in the given date range
            var orders = _db.Orders
                .Include(o => o.user) 
                .Include(o => o.OrderProducts)
                .ThenInclude(op => op.product)
                .Where(o => o.OrderDate >= from && o.OrderDate < to);

            // Total number of orders
            var totalOrders = await orders.CountAsync();

            // Total revenue from orders
            var totalRevenue = await orders.SumAsync(o => o.TotalAmount);

            // Top 5 products by quantity sold
            //var topProducts = await _db.OrderProducts
            //    .Include(op => op.product)
            //    .Where(op => op.Order.OrderDate >= from && op.Order.OrderDate < to)
            //    .GroupBy(op => new { op.OID, op.product.ProductName })
            //    .Select(g => new
            //    {
            //        ProductId = g.Key.PID,           
            //        Name = g.Key.ProductName,       
            //        Quantity = g.Sum(x => x.Quantity)
            //    })
            //    .OrderByDescending(x => x.Quantity)
            //    .Take(5)
            //    .ToListAsync();

            // Top 5 customers by order count
            var topCustomers = await orders
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
                .ToListAsync();

            // Final summary object
            return new
            {
                totalOrders,
                totalRevenue,
                //topProducts,
                topCustomers
            };
        }
    }
}
