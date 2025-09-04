
namespace E_CommerceSystem.Repositories
{
    public interface IOrderSummaryRepo
    {
        Task<List<object>> GetTopCustomersAsync(DateTime from, DateTime to);
        Task<int> GetTotalOrdersAsync(DateTime from, DateTime to);
        Task<decimal> GetTotalRevenueAsync(DateTime from, DateTime to);
    }
}