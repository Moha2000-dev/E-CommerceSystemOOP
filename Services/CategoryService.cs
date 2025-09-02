using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;
using static E_CommerceSystem.Models.CategoryDtos;
using AutoMapper;
using E_CommerceSystem.Models;
using E_CommerceSystem.Infrastructure.Repositories;
using E_CommerceSystem.Models;
namespace E_CommerceSystem.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repo;
        private readonly IMapper _mapper;
        public CategoryService(ICategoryRepository repo, IMapper mapper) { _repo = repo; _mapper = mapper; }

        public async Task<List<CategoryDto>> GetAllAsync() =>
            (await _repo.GetAllAsync()).Select(_mapper.Map<CategoryDto>).ToList();

        public async Task<CategoryDto?> GetAsync(int id)
            => (await _repo.GetAsync(id)) is { } e ? _mapper.Map<CategoryDto>(e) : null;

        public async Task<int> CreateAsync(CreateCategoryDto dto)
        {
            var entity = _mapper.Map<Category>(dto);
            await _repo.AddAsync(entity);
            await _repo.SaveAsync();
            return entity.CategoryId;
        }
        public async Task<bool> UpdateAsync(int id, UpdateCategoryDto dto)
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
