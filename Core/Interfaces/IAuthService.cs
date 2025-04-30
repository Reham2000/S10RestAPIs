using Domain.DTos;
using Domain.Models;

namespace Core.Interfaces
{
    public interface IAuthService
    {
        Task<User> RegisterAsync(RegisterDTo model);
        Task<User> LoginAsync(LoginDTo model);
        Task<bool> UserExist(string userName);
        Task StoreJwtToken(User user , string token);
    }
}
