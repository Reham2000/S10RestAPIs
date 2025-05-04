using Domain.DTos;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IProductRepository products { get; }
        IRevokedTokenRepository revokedTokens { get; }
        IRefreshTokenRepository refreshTokens { get; }

        UserManager<User> userManager { get; }
        SignInManager<User> signInManager { get; }
        RoleManager<IdentityRole> roleManager { get; }
        IHttpContextAccessor httpContextAccessor { get; }
        Jwt jwt { get; }


        Task<int> CompleteAsync();
    }
}
