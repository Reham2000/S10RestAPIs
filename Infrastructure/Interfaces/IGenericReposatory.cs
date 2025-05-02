using System.Linq.Expressions;

namespace Infrastructure.Interfaces
{
    public interface IGenericReposatory<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
        Task<T> GetByCritriaAsync(Expression<Func<T,bool>> criteria );
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        //Task DeleteAsync(int id);
        Task DeleteAsync(T entity);
    }
}
