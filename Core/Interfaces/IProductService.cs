using Domain.Models;

namespace Core.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product> GetByIdAsync(int id);
        Task AddAsync(Product model);
        Task UpdateAsync(Product model);
        Task DeleteAsync(int id);
    }
}
