using Domain.DTos;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface ITokenService
    {
        string GenerateJwtToken(User user);


        // RefreshToken
        RefreshToken GenerateRefreshToken(string ipAddress);
        Task<AuthenticationResponse> RefreshToken(string token,string ipAddress);
        Task<bool> RevokeToken(string token , string ipAddress);
    }
}
