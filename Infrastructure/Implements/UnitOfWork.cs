using Domain.DTos;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.Extensions.Options;

namespace Infrastructure.Implements
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly IOptions<Jwt> _jwt;
        public UnitOfWork(AppDbContext context, IOptions<Jwt> jwt)
        {
            _context = context;
            _jwt = jwt;
            products = new ProductRepository(_context);
            revokedTokens = new RevokedTokenRepository(_context,_jwt);
        }


        public IProductRepository products { get; private set; }
        public IRevokedTokenRepository revokedTokens { get; private set; }

        public async Task<int> CompleteAsync() =>  await _context.SaveChangesAsync();

        public void Dispose()
        {
            _context.Dispose();
        }

    }
}
