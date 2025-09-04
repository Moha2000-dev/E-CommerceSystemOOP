using E_CommerceSystem.Models;
using System.Linq;

namespace E_CommerceSystem.Repositories
{
    public interface IProductRepo
    {
        void AddProduct(Product product);
        IEnumerable<Product> GetAllProducts();
        Product GetProductById(int pid);
        void UpdateProduct(Product product);
        Product GetProductByName(string productName);
        IQueryable<Product> QueryProducts();
    }
}