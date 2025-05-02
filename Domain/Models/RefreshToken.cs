using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public bool IsExpired => DateTime.Now >= Expires;
        public DateTime Created { get; set; } = DateTime.Now;
        public string CreatedByIp { get; set; }
        public DateTime? Revoced { get; set; }
        public string? RevocedByIp { get; set; }

        public bool IsActive => Revoced == null && !IsExpired;
        public string UserId { get; set; }
        [ValidateNever]
        public User User { get; set; }
    }
}
