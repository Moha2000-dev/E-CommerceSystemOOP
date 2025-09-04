using E_CommerceSystem.Repositories;

namespace E_CommerceSystem.Services
{
    public class OrderSummaryService : IOrderSummaryService
    {
        private readonly IOrderSummaryRepo _orderSummaryRepo;

        public OrderSummaryService(IOrderSummaryRepo orderSummaryRepo)
        {
            _orderSummaryRepo = orderSummaryRepo;
        }

        public async Task<object> GetSummaryAsync(DateTime from, DateTime to)
        {
            var totalOrders = await _orderSummaryRepo.GetTotalOrdersAsync(from, to);
            var totalRevenue = await _orderSummaryRepo.GetTotalRevenueAsync(from, to);
            var topCustomers = await _orderSummaryRepo.GetTopCustomersAsync(from, to);
            // var topProducts = await _orderSummaryRepo.GetTopProductsAsync(from, to);

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
