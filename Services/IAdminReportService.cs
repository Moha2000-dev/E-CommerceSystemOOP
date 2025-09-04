using E_CommerceSystem.DTOs;

namespace E_CommerceSystem.Services
{
    public interface IAdminReportService
    {
        Task<IReadOnlyList<BestSellingProductDto>> BestSellingAsync(DateTime? from, DateTime? to, int top = 10);
        Task<IReadOnlyList<RevenuePointDto>> RevenueDailyAsync(DateTime from, DateTime to);
        Task<IReadOnlyList<RevenueMonthDto>> RevenueMonthlyAsync(int year);
        Task<IReadOnlyList<TopRatedProductDto>> TopRatedAsync(int minReviews = 1, int top = 10);
        Task<IReadOnlyList<ActiveCustomerDto>> MostActiveAsync(DateTime? from, DateTime? to, int top = 10);
    }
}