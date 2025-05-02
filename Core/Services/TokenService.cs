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

namespace Core.Services
{
    public class TokenService : ITokenService
    {
        private readonly Jwt _jwt;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        public TokenService(IOptions<Jwt> jwt,IUnitOfWork unitOfWork,UserManager<User> userManager)
        {
            _jwt = jwt.Value;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        public string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub,user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier,user.Id),
                new Claim(ClaimTypes.Email,user.Email)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secretkey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddMinutes(_jwt.ExpiryDurationInMinutes);
            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims : claims,
                expires: expires,
                signingCredentials:creds
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public RefreshToken GenerateRefreshToken(string ipAddress)
        {
            return new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.Now.AddMinutes(_jwt.ExpiryDurationInMinutes),
                Created = DateTime.Now,
                CreatedByIp = ipAddress,
            };
        }

        public async Task<AuthenticationResponse> RefreshToken(string token, string ipAddress)
        {
            var refreshToken = await _unitOfWork.refreshTokens.GetByCritriaAsync(r => r.Token == token);
            if (refreshToken == null || !refreshToken.IsActive)
                return null;

            var user = await _userManager.FindByIdAsync(refreshToken.UserId);

            var newRefreshToken = GenerateRefreshToken(ipAddress);
            refreshToken.Revoced = DateTime.Now;
            refreshToken.RevocedByIp = ipAddress;

            user.RefreshTokens.Add(refreshToken);
            await _unitOfWork.CompleteAsync();

            var jwtToken =  GenerateJwtToken(user);
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
