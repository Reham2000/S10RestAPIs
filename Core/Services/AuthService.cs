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
        // Old DI
        //private readonly UserManager<User> _unitOfWork.userManager;
        //private readonly SignInManager<User> _signInManager;
        //private readonly IHttpContextAccessor _contextAccessor;
        //private readonly Jwt _jwt;
        //private readonly IUnitOfWork _unitOfWork;
        //private readonly RoleManager<IdentityRole> _roleManager;
        //public AuthService(UserManager<User> userManager,SignInManager<User> signInManager,
        //    IHttpContextAccessor contextAccessor,IOptions<Jwt> jwt,IUnitOfWork unitOfWork,
        //    RoleManager<IdentityRole> roleManager)
        //{
        //    _unitOfWork.userManager = userManager;
        //    _signInManager = signInManager;
        //    _contextAccessor = contextAccessor;
        //    _jwt = jwt.Value;
        //    _unitOfWork = unitOfWork;
        //    _roleManager = roleManager;
        //}
        // new DI
        private readonly IUnitOfWork _unitOfWork;
        public AuthService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<ReturnModel<User>> RegisterAsync(RegisterDTo model)
        {
            if(await UserExist(model.UserName))
                return new ReturnModel<User>
                {
                    IsSuccessed = false,
                    Errors = new List<string> { "UserName Is Already Exist!" }
                };

            var user = new User
            {
                UserName = model.UserName,
                Email = model.Email

            };

            var result = await _unitOfWork.userManager.CreateAsync(user, model.Password);
            if(result.Succeeded)
            {
                await _unitOfWork.userManager.AddToRoleAsync(user,"User");
                return new ReturnModel<User>
                {
                    IsSuccessed = true,
                    Model = user
                };
            }
            return new ReturnModel<User>
            {
                IsSuccessed = false,
                Errors = result.Errors.Select(e => e.Description).ToList()
            };

        }
        public async Task<ReturnModel<User>> AddAsync(UserDTo model)
        {
            if (await UserExist(model.UserName))
                return new ReturnModel<User>
                {
                    IsSuccessed = false,
                    Errors = new List<string> { "UserName Is Already Exist!"}
                };
            var user = new User
            {
                UserName = model.UserName,
                Email = model.Email

            };

            var result = await _unitOfWork.userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                var role = await _unitOfWork.roleManager.FindByIdAsync(model.RoleId);
                if (role == null)
                    return new ReturnModel<User>
                    {
                        IsSuccessed = false,
                        Errors = new List<string> { "Invaild Role!" }
                    };
                await _unitOfWork.userManager.AddToRoleAsync(user, role.Name);
                return new ReturnModel<User>
                {
                    IsSuccessed = true,
                   Model = user
                };
            }
            return new ReturnModel<User>
            {
                IsSuccessed = false,
                Errors = result.Errors.Select(e => e.Description).ToList()
            };

        }
        public async Task<ReturnModel<User>> LoginAsync(LoginDTo model)
        {
            try
            {
                var user = await _unitOfWork.userManager.FindByNameAsync(model.UserName);
                if(user is not null && await _unitOfWork.userManager.CheckPasswordAsync(user,model.Password))
                {
                    await _unitOfWork.signInManager.SignInAsync(user, isPersistent: false);
                    return new ReturnModel<User>
                    {
                        IsSuccessed = true,
                        Model = user
                    };

                }
                return new ReturnModel<User>
                {
                    IsSuccessed = false,
                    Errors = new List<string> { "Invaild UserName Or Password!" }
                };
            }
            catch (Exception ex) {
                return new ReturnModel<User>
                {
                    IsSuccessed = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }
        public async Task<bool> UserExist(string userName)
        {
            var user = await _unitOfWork.userManager.FindByNameAsync(userName);
            if(user == null)
                return false;
            return true;
        }
        public async Task StoreJwtToken(User user, string token)
        {
            await _unitOfWork.userManager.SetAuthenticationTokenAsync(user,"Jwt","Access Token", token);

            // store token in refreshToken table
            string userIpAddress = _unitOfWork.httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var refreshToken = new RefreshToken
            {
                Token = token,
                Expires = DateTime.Now.AddMinutes(_unitOfWork.jwt.ExpiryDurationInMinutes),
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
