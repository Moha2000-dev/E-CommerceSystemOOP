using static E_CommerceSystem.Models.PagingDtos;

namespace E_CommerceSystem.Services
{
    public interface IProductQueryService
    {
        Task<PagedResult<ProductListDto>> GetAsync(string? name, decimal? minPrice, decimal? maxPrice, int page, int pageSize);
    }
}
