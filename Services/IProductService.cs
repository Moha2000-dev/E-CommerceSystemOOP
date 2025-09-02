using E_CommerceSystem.Models;
using static E_CommerceSystem.Models.PagingDtos;

namespace E_CommerceSystem.Services
{
    public interface IProductService
    {
        void AddProduct(Product product);
        PagedResult<ProductListDto> GetAllProducts(int pageNumber, int pageSize,
          string? name, decimal? minPrice, decimal? maxPrice);
       
        Product GetProductById(int pid);
        void UpdateProduct(Product product);
        Product GetProductByName(string productName);
    }
}