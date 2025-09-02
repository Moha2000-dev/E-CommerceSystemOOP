using E_CommerceSystem.Models;

namespace E_CommerceSystem.Repositories
{
    public interface ICategoryRepository
    {
        Task<Category?> GetAsync(int id);
        Task<List<Category>> GetAllAsync();
        Task AddAsync(Category entity);
        void Update(Category entity);
        void Delete(Category entity);
        Task<int> SaveAsync();
    }
}
