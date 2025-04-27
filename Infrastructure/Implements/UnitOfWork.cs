using Infrastructure.Data;
using Infrastructure.Interfaces;

namespace Infrastructure.Implements
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            products = new ProductRepository(_context);
        }


        public IProductRepository products { get; private set; }

        public async Task<int> CompleteAsync() =>  await _context.SaveChangesAsync();

        public void Dispose()
        {
            _context.Dispose();
        }

    }
}
