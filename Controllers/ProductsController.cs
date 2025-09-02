using System.Threading.Tasks;

using E_CommerceSystem.Models;
using E_CommerceSystem.Services;
using Microsoft.AspNetCore.Mvc;
using static E_CommerceSystem.Models.PagingDtos;

namespace E_CommerceSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    /// <summary>List products with optional filters and pagination.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductDTO>), 200)]
    public async Task<ActionResult<PagedResult<ProductDTO>>> List(
        [FromQuery] string? name,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromServices] IProductQueryService svc = null!)
    {
        var result = await svc.GetAsync(name, minPrice, maxPrice, page, pageSize);
        return Ok(result);
    }
}
