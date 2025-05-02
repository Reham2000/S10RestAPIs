using Core.Interfaces;
using Domain.DTos;
using Domain.Models;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly Jwt _jwt;
        private readonly IUnitOfWork _unitOfWork;
        public AuthService(UserManager<User> userManager,SignInManager<User> signInManager,
            IHttpContextAccessor contextAccessor,IOptions<Jwt> jwt,IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _contextAccessor = contextAccessor;
            _jwt = jwt.Value;
            _unitOfWork = unitOfWork;
        }
        public async Task<User> RegisterAsync(RegisterDTo model)
        {
            if(await UserExist(model.UserName))
                return null;

            var user = new User
            {
                UserName = model.UserName,
                Email = model.Email

            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if(result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user,"User");
                return user; 
            }
            return null;

        }
        public async Task<User> LoginAsync(LoginDTo model)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(model.UserName);
                if(user is not null && await _userManager.CheckPasswordAsync(user,model.Password))
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return user;
                    
                }
                return null;
            }
            catch (Exception ex) {
                return null;
            }
        }
        public async Task<bool> UserExist(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if(user == null)
                return false;
            return true;
        }
        public async Task StoreJwtToken(User user, string token)
        {
            await _userManager.SetAuthenticationTokenAsync(user,"Jwt","Access Token", token);

            // store token in refreshToken table
            string userIpAddress = _contextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var refreshToken = new RefreshToken
            {
                Token = token,
                Expires = DateTime.Now.AddMinutes(_jwt.ExpiryDurationInMinutes),
                Created = DateTime.Now,
                CreatedByIp = userIpAddress,
                UserId = user.Id,
                //RevocedByIp = string.Empty,

            };
            await _unitOfWork.refreshTokens.AddAsync(refreshToken);
            await _unitOfWork.CompleteAsync();
        
        }
    }
}
