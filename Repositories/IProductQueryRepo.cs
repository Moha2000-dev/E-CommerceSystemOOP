using E_CommerceSystem.Models;

namespace E_CommerceSystem.Repositories
{
    public interface IProductQueryRepo
    {
        IQueryable<Product> QueryProducts();
    }
}