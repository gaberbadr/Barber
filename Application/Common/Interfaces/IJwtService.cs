using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Interfaces
{
    public interface IJwtService
    {
        Task<(string token, DateTime expiresAt)> GenerateAccessTokenAsync(
          ApplicationUser user,
          UserManager<ApplicationUser> userManager
      );

        (string token, DateTime expiresAt) GenerateRefreshToken();
    }
}
