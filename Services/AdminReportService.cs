using E_CommerceSystem.DTOs;

namespace E_CommerceSystem.Services
{

    public class AdminReportService : IAdminReportService
    {
        private readonly Repositories.IAdminReportRepo _repo;
        public AdminReportService(Repositories.IAdminReportRepo repo) => _repo = repo;

        public Task<IReadOnlyList<BestSellingProductDto>> BestSellingAsync(DateTime? from, DateTime? to, int top = 10)
            => _repo.GetBestSellingAsync(from, to, top);

        public Task<IReadOnlyList<RevenuePointDto>> RevenueDailyAsync(DateTime from, DateTime to)
            => _repo.GetDailyRevenueAsync(from, to);

        public Task<IReadOnlyList<RevenueMonthDto>> RevenueMonthlyAsync(int year)
            => _repo.GetMonthlyRevenueAsync(year);

        public Task<IReadOnlyList<TopRatedProductDto>> TopRatedAsync(int minReviews = 1, int top = 10)
            => _repo.GetTopRatedProductsAsync(minReviews, top);

        public Task<IReadOnlyList<ActiveCustomerDto>> MostActiveAsync(DateTime? from, DateTime? to, int top = 10)
            => _repo.GetMostActiveCustomersAsync(from, to, top);
    }
}

