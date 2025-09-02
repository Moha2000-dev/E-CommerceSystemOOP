using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceSystem.Infrastructure.Repositories;
public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _db;
    public CategoryRepository(ApplicationDbContext db) => _db = db;

    public Task<Category?> GetAsync(int id) => _db.Categories.FindAsync(id).AsTask();
    public Task<List<Category>> GetAllAsync() => _db.Categories.AsNoTracking().ToListAsync();
    public Task AddAsync(Category e) => _db.Categories.AddAsync(e).AsTask();
    public void Update(Category e) => _db.Categories.Update(e);
    public void Delete(Category e) => _db.Categories.Remove(e);
    public Task<int> SaveAsync() => _db.SaveChangesAsync();
}