using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Authorization
{
    public class CustomAuthorizationRequirement : IAuthorizationRequirement
    {
        public List<string> AllowedRoles { get;}
        public CustomAuthorizationRequirement(List<string> allwedRoles)
        {
            AllowedRoles = allwedRoles ?? new List<string>();    
        }
    }

}
