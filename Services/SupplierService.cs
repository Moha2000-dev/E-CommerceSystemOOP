using AutoMapper;
using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;
using static E_CommerceSystem.Models.SupplierDtos;

namespace E_CommerceSystem.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly ISupplierRepository _repo;
        private readonly IMapper _mapper;
        public SupplierService(ISupplierRepository repo, IMapper mapper) { _repo = repo; _mapper = mapper; }

        public async Task<List<SupplierDto>> GetAllAsync() =>
            (await _repo.GetAllAsync()).Select(_mapper.Map<SupplierDto>).ToList();

        public async Task<SupplierDto?> GetAsync(int id)
            => (await _repo.GetAsync(id)) is { } e ? _mapper.Map<SupplierDto>(e) : null;

        public async Task<int> CreateAsync(CreateSupplierDto dto)
        {
            var entity = _mapper.Map<Supplier>(dto);
            await _repo.AddAsync(entity);
            await _repo.SaveAsync();
            return entity.SupplierId;
        }
        public async Task<bool> UpdateAsync(int id, UpdateSupplierDto dto)
        {
            var e = await _repo.GetAsync(id); if (e is null) return false;
            _mapper.Map(dto, e); _repo.Update(e); await _repo.SaveAsync(); return true;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var e = await _repo.GetAsync(id); if (e is null) return false;
            _repo.Delete(e); await _repo.SaveAsync(); return true;
        }
    }
}
