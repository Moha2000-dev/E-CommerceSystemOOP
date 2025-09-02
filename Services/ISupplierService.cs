using static E_CommerceSystem.Models.SupplierDtos;

namespace E_CommerceSystem.Services
{
    public interface ISupplierService
    {
        Task<List<SupplierDto>> GetAllAsync();
        Task<SupplierDto?> GetAsync(int id);
        Task<int> CreateAsync(CreateSupplierDto dto);
        Task<bool> UpdateAsync(int id, UpdateSupplierDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
