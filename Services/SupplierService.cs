// Services/SupplierService.cs
using AutoMapper;
using AutoMapper.QueryableExtensions;
using E_CommerceSystem;
using E_CommerceSystem.Models;
using E_CommerceSystem.Services;
using Microsoft.EntityFrameworkCore;
using static E_CommerceSystem.Models.SupplierDtos;

public class SupplierService : ISupplierService
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;

    public SupplierService(ApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<List<SupplierDto>> GetAllAsync() =>
        await _db.Suppliers.AsNoTracking()
            .ProjectTo<SupplierDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

    public async Task<SupplierDto?> GetAsync(int id) =>
        await _db.Suppliers.AsNoTracking()
            .Where(s => s.SupplierId == id)
            .ProjectTo<SupplierDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();

    public async Task<int> CreateAsync(CreateSupplierDto dto)
    {
        var entity = _mapper.Map<Supplier>(dto);
        _db.Suppliers.Add(entity);
        await _db.SaveChangesAsync();
        return entity.SupplierId;
    }

    public async Task<bool> UpdateAsync(int id, UpdateSupplierDto dto)
    {
        var entity = await _db.Suppliers.FindAsync(id);
        if (entity is null) return false;

        _mapper.Map(dto, entity);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.Suppliers.FindAsync(id);
        if (entity is null) return false;

        _db.Suppliers.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }
}
