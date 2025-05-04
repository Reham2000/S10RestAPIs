using Core.Interfaces;
using Domain.DTos;
using Domain.Models;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    public class TokenService : ITokenService
    {
        //private readonly Jwt _unitOfWork.jwt;
        private readonly IUnitOfWork _unitOfWork;
        //private readonly UserManager<User> _unitOfWork.userManager;
        public TokenService(/*IOptions<Jwt> jwt,*/IUnitOfWork unitOfWork/*,UserManager<User> userManager*/)
        {
            //_unitOfWork.jwt = jwt.Value;
            _unitOfWork = unitOfWork;
            //_unitOfWork.userManager = userManager;
        }
        public async Task<string> GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub,user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier,user.Id),
                new Claim(ClaimTypes.Email,user.Email)
            };
            var roles = await _unitOfWork.userManager.GetRolesAsync(user);
            foreach(var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role,role));

            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_unitOfWork.jwt.Secretkey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddMinutes(_unitOfWork.jwt.ExpiryDurationInMinutes);
            var token = new JwtSecurityToken(
                issuer: _unitOfWork.jwt.Issuer,
                audience: _unitOfWork.jwt.Audience,
                claims : claims,
                expires: expires,
                signingCredentials:creds
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public RefreshToken GenerateRefreshToken(string ipAddress,string jwtToken = null)
        {
            return new RefreshToken
            {
                Token = jwtToken == null ?
                Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))  :
                 jwtToken,
                Expires = DateTime.Now.AddMinutes(_unitOfWork.jwt.ExpiryDurationInMinutes),
                Created = DateTime.Now,
                CreatedByIp = ipAddress,
            };
        }

        public async Task<AuthenticationResponse> RefreshToken(string token, string ipAddress)
        {
            var refreshToken = await _unitOfWork.refreshTokens.GetByCritriaAsync(r => r.Token == token);
            if (refreshToken == null || !refreshToken.IsActive)
                return null;

            var user = await _unitOfWork.userManager.FindByIdAsync(refreshToken.UserId);

            var jwtToken = await GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken(ipAddress,jwtToken);
            refreshToken.Revoced = DateTime.Now;
            refreshToken.RevocedByIp = ipAddress;

            user.RefreshTokens.Add(refreshToken);
            await _unitOfWork.CompleteAsync();

            return new AuthenticationResponse(jwtToken,newRefreshToken.Token);

        }

        public async Task<bool> RevokeToken(string token, string ipAddress)
        {
            var refreshToken = await _unitOfWork.refreshTokens.GetByCritriaAsync(r => r.Token == token);
            if(refreshToken is null || !refreshToken.IsActive) 
                return false;

            refreshToken.Revoced = DateTime.Now;
            refreshToken.RevocedByIp = ipAddress;
            await _unitOfWork.CompleteAsync();
            return true;

        }
    }
}
