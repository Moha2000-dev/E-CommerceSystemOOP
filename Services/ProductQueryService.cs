using AutoMapper;
using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions; // <-- enables .ProjectTo(...)
using static E_CommerceSystem.Models.PagingDtos;

namespace E_CommerceSystem.Services
{
   
        public class ProductQueryService : IProductQueryService
        {
            private readonly ApplicationDbContext _db;
            private readonly IMapper _mapper;
            public ProductQueryService(ApplicationDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

            public async Task<PagedResult<ProductListDto>> GetAsync(string? name, decimal? minPrice, decimal? maxPrice, int page, int pageSize)
            {
                if (page <= 0) page = 1;
                if (pageSize <= 0 || pageSize > 100) pageSize = 20;

                var q = _db.Products
                    .Include(p => p.Category)
                    .Include(p => p.Supplier)
                    .AsNoTracking()
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(name)) q = q.Where(p => p.ProductName.Contains(name));
                if (minPrice.HasValue) q = q.Where(p => p.Price >= minPrice.Value);
                if (maxPrice.HasValue) q = q.Where(p => p.Price <= maxPrice.Value);

                var total = await q.CountAsync();
                var items = await q.OrderBy(p => p.PID)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ProjectTo<ProductListDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                return new PagedResult<ProductListDto>(items, total, page, pageSize);
            }
        }

    }

