using E_CommerceSystem.DTOs;

namespace E_CommerceSystem.Repositories
{
    public interface IAdminReportRepo
    {
        Task<IReadOnlyList<BestSellingProductDto>> GetBestSellingAsync(DateTime? from, DateTime? to, int top);
        Task<IReadOnlyList<RevenuePointDto>> GetDailyRevenueAsync(DateTime from, DateTime to);
        Task<IReadOnlyList<RevenueMonthDto>> GetMonthlyRevenueAsync(int year);
        Task<IReadOnlyList<TopRatedProductDto>> GetTopRatedProductsAsync(int minReviews, int top);
        Task<IReadOnlyList<ActiveCustomerDto>> GetMostActiveCustomersAsync(DateTime? from, DateTime? to, int top);
    }
}
