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

        //2-
       // Task<Result> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
       /*     Task<Result> ConfirmEmailAsync(string userId, string token, CancellationToken cancellationToken = default);
            Task<Result> ForgotPasswordAsync(string email, string origin, CancellationToken cancellationToken = default);
            Task<Result> ResetPasswordAsync(ResetPasswordRequest model, CancellationToken cancellationToken = default);
            Task<Result> ChangePasswordAsync(ChangePasswordRequest model, CancellationToken cancellationToken = default);*/
    }
}
