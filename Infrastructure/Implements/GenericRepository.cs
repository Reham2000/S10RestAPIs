using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Implements
{
    public class GenericRepository<T> : IGenericReposatory<T> where T : class
    {

        private readonly AppDbContext _context;
        public GenericRepository(AppDbContext context)
        {
            _context = context;
        }
        //  _context.Set<T>() // T table     // product      => products
        public async Task<IEnumerable<T>> GetAllAsync() => await _context.Set<T>().ToListAsync();
        public async Task<T> GetByIdAsync(int id) => await _context.Set<T>().FindAsync(id);
        public async Task AddAsync(T entity) => await _context.Set<T>().AddAsync(entity);
        public async Task UpdateAsync(T entity) =>  _context.Set<T>().Update(entity);
        //public async Task DeleteAsync(int id) => _context.Set<T>().Remove(await GetByIdAsync(id));
        public async Task DeleteAsync(T entity) => _context.Set<T>().Remove(entity);  

    }
}
