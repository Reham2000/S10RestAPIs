using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Domain.Models
{
    public class User : IdentityUser
    {
        [ValidateNever]
        public List<RefreshToken> RefreshTokens { get; set; } = new();
    }
}
