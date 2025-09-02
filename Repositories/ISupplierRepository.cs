using E_CommerceSystem.Models;
namespace E_CommerceSystem.Repositories;
public interface ISupplierRepository
{
    Task<Supplier?> GetAsync(int id);
    Task<List<Supplier>> GetAllAsync();
    Task AddAsync(Supplier entity);
    void Update(Supplier entity);
    void Delete(Supplier entity);
    Task<int> SaveAsync();
}