using Domain.DTos;
using Domain.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface ITokenService
    {
        Task<string> GenerateJwtToken(User user);


        // RefreshToken
        RefreshToken GenerateRefreshToken(string ipAddress,string jwtToken = null);
        Task<AuthenticationResponse> RefreshToken(string token,string ipAddress);
        Task<bool> RevokeToken(string token , string ipAddress);
    }
}
