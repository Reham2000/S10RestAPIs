using Domain.Models;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Implements
{
    public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
    {
        private readonly AppDbContext _context;
        public RefreshTokenRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<RefreshToken> GetActiveRefreshTokenAsync(string token)
        {
            var result = await _context.RefreshTokens.Where(r => r.Token == token).ToListAsync();

            return result.FirstOrDefault(r => r.IsActive);
        }
    }
}
