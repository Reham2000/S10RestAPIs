using Domain.DTos;
using Domain.Models;

namespace Core.Interfaces
{
    public interface IAuthService
    {
        Task<ReturnModel<User>> RegisterAsync(RegisterDTo model);
        Task<ReturnModel<User>> AddAsync(UserDTo model);
        Task<ReturnModel<User>> LoginAsync(LoginDTo model);
        Task<bool> UserExist(string userName);
        Task StoreJwtToken(User user , string token);
    }
}
