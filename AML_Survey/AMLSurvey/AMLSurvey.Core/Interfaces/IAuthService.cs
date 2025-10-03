using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AMLSurvey.Core.Abstractions;
using AMLSurvey.Core.DTOS.Auth;

namespace AMLSurvey.Core.Interfaces
{
    public interface IAuthService
    {
        //1- token and refresh token
            Task<Result<AuthResponse>> GetTokenAsync(string email, string password,
                CancellationToken cancellationToken = default);

            Task<Result<AuthResponse>> GetRefreshTokenAsync(string token , string refreshToken, CancellationToken cancellationToken = default);

            Task<Result> RevokeRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default);
    }
}
