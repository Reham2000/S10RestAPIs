using Domain.DTos;
using Domain.Models;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure.Implements
{
    public class RevokedTokenRepository : GenericRepository<RevokedToken>, IRevokedTokenRepository
    {
        private readonly AppDbContext _context;
        private readonly Jwt _jwt;

        public RevokedTokenRepository(AppDbContext context,IOptions<Jwt> jwt) : base(context) 
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context), "AppDbContext is null");

            if (jwt == null)
                throw new ArgumentNullException(nameof(jwt), "IOptions<Jwt> is null");

            if (jwt.Value == null)
                throw new ArgumentNullException(nameof(jwt.Value), "Jwt value is null");
            _context = context;
            _jwt = jwt.Value;
        }
        public async Task<bool> IsTokenRevokedAsync(string jti)
        {
            return await _context.RevokedTokens.AnyAsync(r => r.Jti == jti);
        }

        public async Task RevokTokenAsync(string jti)
        {
            await _context.RevokedTokens.AddAsync(
                new RevokedToken
                {
                    Jti = jti,
                    ExpirationDate = DateTime.Now.AddMinutes(_jwt.ExpiryDurationInMinutes)
                }
                );
        }
    }
}
