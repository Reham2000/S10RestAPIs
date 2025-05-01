using Core.Interfaces;
using Domain.DTos;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
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
        public AuthService(UserManager<User> userManager,SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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
        }
    }
}
