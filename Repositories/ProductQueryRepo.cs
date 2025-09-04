using E_CommerceSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceSystem.Repositories
{
    public class ProductQueryRepo : IProductQueryRepo
    {
        private readonly ApplicationDbContext _db;

        public ProductQueryRepo(ApplicationDbContext db)
        {
            _db = db;
        }

        public IQueryable<Product> QueryProducts()
        {
            // Repo responsibility: expose EF query
            return _db.Products.AsNoTracking();
        }
    }
}
