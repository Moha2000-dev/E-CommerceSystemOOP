using E_CommerceSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceSystem.Repositories;
public class SupplierRepository : ISupplierRepository
{
    private readonly ApplicationDbContext _db;
    public SupplierRepository(ApplicationDbContext db) => _db = db;

    public Task<Supplier?> GetAsync(int id) => _db.Suppliers.FindAsync(id).AsTask();
    public Task<List<Supplier>> GetAllAsync() => _db.Suppliers.AsNoTracking().ToListAsync();
    public Task AddAsync(Supplier e) => _db.Suppliers.AddAsync(e).AsTask();
    public void Update(Supplier e) => _db.Suppliers.Update(e);
    public void Delete(Supplier e) => _db.Suppliers.Remove(e);
    public Task<int> SaveAsync() => _db.SaveChangesAsync();
}