using AMLSurvey.Core.Abstractions;
using AMLSurvey.Core.DTOS.Auth;
using AMLSurvey.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMLSurvey.Core.Services
{
    public class AuthService : IAuthService
    {
       // private readonly UserManager<ApplicationUser> _userManager;
        public Task<Result<AuthResponse>> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            /* if (await _userManager.FindByEmailAsync(email) is not { } user)
                 return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

             if (user.IsDisabled)*/
            /* return Result.Failure<AuthResponse>(UserErrors.DisabledUser);*/

            throw new NotImplementedException();
        }

        public Task<Result<AuthResponse>> GetRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Result> RevokeRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
