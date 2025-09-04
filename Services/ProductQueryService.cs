// Services/ProductQueryService.cs
using AutoMapper;
using AutoMapper.QueryableExtensions;             // <-- IMPORTANT
using E_CommerceSystem;
using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;
using E_CommerceSystem.Services;
using Microsoft.EntityFrameworkCore;
using static E_CommerceSystem.Models.PagingDtos;

public class ProductQueryService : IProductQueryService
{
    private readonly IProductQueryRepo _repo;
    private readonly IMapper _mapper;

    public ProductQueryService(IProductQueryRepo repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }
    public async Task<PagedResult<ProductDTO>> GetAsync(
            string? name, decimal? minPrice, decimal? maxPrice, int page, int pageSize)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 20;

        //  get base query from repo
        var q = _repo.QueryProducts();

        // Filtering (business logic, stays in service)
        if (!string.IsNullOrWhiteSpace(name)) q = q.Where(p => p.ProductName.Contains(name));
        if (minPrice.HasValue) q = q.Where(p => p.Price >= minPrice.Value);
        if (maxPrice.HasValue) q = q.Where(p => p.Price <= maxPrice.Value);

        var total = await q.CountAsync();

        // Mapping to DTOs
        var items = await q.OrderBy(p => p.PID)
            .ProjectTo<ProductDTO>(_mapper.ConfigurationProvider)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<ProductDTO>(items, total, page, pageSize);
    }
}
