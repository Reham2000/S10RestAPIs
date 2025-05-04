using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    public class ServiceUnitOfWork : IServiceUnitOfWork
    {
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly IProductService _productService;
        public ServiceUnitOfWork(IAuthService authService,ITokenService tokenService,
            IProductService productService)
        {
            _authService = authService;
            _tokenService = tokenService;
            _productService = productService;
            
        }


        public IAuthService authService => _authService;
        public ITokenService tokenService => _tokenService;
        public IProductService productService => _productService;

        
    }
}
