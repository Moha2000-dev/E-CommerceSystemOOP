using AutoMapper;
using AutoMapper.QueryableExtensions;
using E_CommerceSystem.Models;
using Microsoft.EntityFrameworkCore;
using static E_CommerceSystem.Models.PagingDtos;

namespace E_CommerceSystem.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public ProductService(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        // Return paged + filtered products (as DTOs)
        public PagedResult<ProductListDto> GetAllProducts(
            int pageNumber,
            int pageSize,
            string? name = null,
            decimal? minPrice = null,
            decimal? maxPrice = null)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _db.Products.AsNoTracking();

            // Filters
            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(p => p.ProductName.Contains(name));
            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            var total = query.Count();

            // Projection with AutoMapper (entity -> ProductListDto)
            var items = query
                .OrderBy(p => p.PID)
                .ProjectTo<ProductListDto>(_mapper.ConfigurationProvider)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<ProductListDto>(items, total, pageNumber, pageSize);
        }

        public Product? GetProductById(int pid)
        {
            return _db.Products.Find(pid);
        }

        public void AddProduct(Product product)
        {
            _db.Products.Add(product);
            _db.SaveChanges();
        }

        public void UpdateProduct(Product product)
        {
            var existing = _db.Products.Find(product.PID);
            if (existing == null)
                throw new KeyNotFoundException($"Product with ID {product.PID} not found.");

            _db.Entry(existing).CurrentValues.SetValues(product);
            _db.SaveChanges();
        }

        public Product GetProductByName(string productName)
        {
            var product = _db.Products.FirstOrDefault(p => p.ProductName == productName);
            if (product == null)
                throw new KeyNotFoundException($"Product with name {productName} not found.");
            return product;
        }
    }
}
