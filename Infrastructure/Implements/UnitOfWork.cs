using Domain.DTos;
using Domain.Models;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Infrastructure.Implements
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly Jwt _jwt;

        // new 
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UnitOfWork(AppDbContext context, IOptions<Jwt> jwt,
            UserManager<User> userManager, RoleManager<IdentityRole> roleManager,
            SignInManager<User> signInManager,IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _jwt = jwt.Value;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;


            products = new ProductRepository(_context);
            revokedTokens = new RevokedTokenRepository(_context,jwt);
            refreshTokens = new RefreshTokenRepository(_context);
        }


        public IProductRepository products { get; private set; }
        public IRevokedTokenRepository revokedTokens { get; private set; }
        public IRefreshTokenRepository refreshTokens { get; private set; }

        // new
        public UserManager<User> userManager => _userManager;
        public SignInManager<User> signInManager => _signInManager;
        public RoleManager<IdentityRole> roleManager => _roleManager;
        public IHttpContextAccessor httpContextAccessor => _httpContextAccessor;
        public Jwt jwt => _jwt;



        public async Task<int> CompleteAsync() =>  await _context.SaveChangesAsync();

        public void Dispose()
        {
            _context.Dispose();
        }

    }
}
