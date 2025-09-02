using System.Collections.Generic;
using System.Threading.Tasks;
using E_CommerceSystem.Services;
using Microsoft.AspNetCore.Mvc;
using static E_CommerceSystem.Models.SupplierDtos;

namespace E_CommerceSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _svc;
    public SuppliersController(ISupplierService svc) => _svc = svc;

    /// <summary>Get all suppliers</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SupplierDto>), 200)]
    public async Task<ActionResult<IEnumerable<SupplierDto>>> GetAll() =>
        Ok(await _svc.GetAllAsync());

    /// <summary>Get a supplier by id</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SupplierDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<SupplierDto>> Get(int id)
        => (await _svc.GetAsync(id)) is { } dto ? Ok(dto) : NotFound();

    /// <summary>Create a new supplier</summary>
    [HttpPost]
    [ProducesResponseType(201)]
    public async Task<ActionResult> Create([FromBody] CreateSupplierDto dto)
    {
        var id = await _svc.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    /// <summary>Update a supplier</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSupplierDto dto)
        => await _svc.UpdateAsync(id, dto) ? NoContent() : NotFound();

    /// <summary>Delete a supplier</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
        => await _svc.DeleteAsync(id) ? NoContent() : NotFound();
}
