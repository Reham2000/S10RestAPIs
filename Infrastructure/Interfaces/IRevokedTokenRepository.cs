using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces
{
    public interface IRevokedTokenRepository
    {
        Task<bool> IsTokenRevokedAsync(string jti);
        Task RevokTokenAsync(string jti);
    }
}
