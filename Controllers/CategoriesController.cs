using System.Collections.Generic;
using System.Threading.Tasks;
using E_CommerceSystem.Services;
using Microsoft.AspNetCore.Mvc;
using static E_CommerceSystem.Models.CategoryDtos;

namespace E_CommerceSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _svc;
    public CategoriesController(ICategoryService svc) => _svc = svc;

    /// <summary>Get all categories</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), 200)]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll() =>
        Ok(await _svc.GetAllAsync());

    /// <summary>Get a category by id</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CategoryDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<CategoryDto>> Get(int id)
        => (await _svc.GetAsync(id)) is { } dto ? Ok(dto) : NotFound();

    /// <summary>Create a new category</summary>
    [HttpPost]
    [ProducesResponseType(201)]
    public async Task<ActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        var id = await _svc.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    /// <summary>Update a category</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto)
        => await _svc.UpdateAsync(id, dto) ? NoContent() : NotFound();

    /// <summary>Delete a category</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
        => await _svc.DeleteAsync(id) ? NoContent() : NotFound();
}
