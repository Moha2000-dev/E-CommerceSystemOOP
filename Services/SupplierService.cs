// Services/SupplierService.cs
using AutoMapper;
using AutoMapper.QueryableExtensions;
using E_CommerceSystem;
using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;
using E_CommerceSystem.Services;
using Microsoft.EntityFrameworkCore;
using static E_CommerceSystem.Models.SupplierDtos;

public class SupplierService : ISupplierService
{
    private readonly ISupplierRepository _repo;
    private readonly IMapper _mapper;

    public SupplierService(ISupplierRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<List<SupplierDto>> GetAllAsync()
    {
        var entities = await _repo.GetAllAsync();
        return _mapper.Map<List<SupplierDto>>(entities);
    }
    public async Task<SupplierDto?> GetAsync(int id)
    {
        var entity = await _repo.GetAsync(id);
        return entity is null ? null : _mapper.Map<SupplierDto>(entity);
    }
    public async Task<int> CreateAsync(CreateSupplierDto dto)
    {
        var entity = _mapper.Map<Supplier>(dto);
        await _repo.AddAsync(entity);
        await _repo.SaveAsync(); //  moved SaveChanges into repo
        return entity.SupplierId;
    }

    public async Task<bool> UpdateAsync(int id, UpdateSupplierDto dto)
    {
        var entity = await _repo.GetAsync(id);
        if (entity is null) return false;

        _mapper.Map(dto, entity);
        _repo.Update(entity);
        await _repo.SaveAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _repo.GetAsync(id);
        if (entity is null) return false;

        _repo.Delete(entity);
        await _repo.SaveAsync();
        return true;
    }
}
