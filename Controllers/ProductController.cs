using AutoMapper;
using E_CommerceSystem.Models;
using E_CommerceSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_CommerceSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IMapper _mapper;

        public ProductController(IProductService productService, IMapper mapper)
        {
            _productService = productService;
            _mapper = mapper;
        }

        // Only admins can add products — no manual token parsing needed
        [Authorize(Roles = "admin")]
        [HttpPost("AddProduct")]
        public IActionResult AddNewProduct([FromBody] ProductDTO productInput)
        {
            if (productInput is null) return BadRequest("Product data is required.");

            // map DTO -> Entity
            var product = _mapper.Map<Product>(productInput);

            // (set FKs here if you pass them separately)
            // product.CategoryId = ...; product.SupplierId = ...;

            _productService.AddProduct(product);
            // map back to DTO so you don't return EF entity
            var result = _mapper.Map<ProductDTO>(product);
            return Ok(result);
        }

        [Authorize(Roles = "admin")]
        [HttpPut("UpdateProduct/{productId:int}")]
        public IActionResult UpdateProduct(int productId, [FromBody] ProductDTO productInput)
        {
            if (productInput is null) return BadRequest("Product data is required.");

            var product = _productService.GetProductById(productId);
            if (product is null) return NotFound("Product not found.");

            // overlay DTO -> tracked entity
            _mapper.Map(productInput, product);

            _productService.UpdateProduct(product);

            var result = _mapper.Map<ProductDTO>(product);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("GetAllProducts")]
        public IActionResult GetAllProducts(
            [FromQuery] string? name,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
                return BadRequest("PageNumber and PageSize must be greater than 0.");

            // Service should already use ProjectTo<ProductListDto>
            var products = _productService.GetAllProducts(pageNumber, pageSize, name, minPrice, maxPrice);
            if (products is null || !products.Items.Any())
                return NotFound("No products found matching the given criteria.");

            return Ok(products);
        }

        [AllowAnonymous]
        [HttpGet("GetProductByID/{productId:int}")]
        public IActionResult GetProductById(int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product is null) return NotFound("No product found.");

            // entity -> DTO
            var dto = _mapper.Map<ProductDTO>(product);
            return Ok(dto);
        }
    }
}
