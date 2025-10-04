using AMLSurvey.Core.Abstractions;
using AMLSurvey.Core.DTOS.Auth;
using AMLSurvey.Core.Entities;
using AMLSurvey.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AMLSurvey.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly int _refreshTokenExpiryDays = 14;
        public AuthService(IUserRepository userRepository, IJwtTokenService jwtTokenService)
        {
            _userRepository = userRepository;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<Result<AuthResponse>> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            // 1. Validate credentials
            var (success, errorCode) = await _userRepository.ValidateCredentialsAsync(email, password, true, cancellationToken);
            
            if (!success)
            {
                var error = errorCode switch
                {
                    "DisabledUser" => new Error("Auth.DisabledUser", "User account is disabled", 0),
                    "EmailNotConfirmed" => new Error("Auth.EmailNotConfirmed", "Email not confirmed",0),
                    "LockedUser" => new Error("Auth.LockedUser", "User account is locked", 0),
                    _ => new Error("Auth.InvalidCredentials", "Invalid email or password",0)
                };
                return Result.Failure<AuthResponse>(error);
            }

            // 2. Get user details
            var user = await _userRepository.FindByEmailAsync(email, cancellationToken);
            if (user is null)
                return Result.Failure<AuthResponse>(new Error("Auth.UserNotFound", "User not found", 0));

            // 3. Get roles and permissions
            var (rolesSuccess, roles, permissions) = await _userRepository.GetUserRolesAndPermissionsAsync(user.Id, cancellationToken);
            
            if (!rolesSuccess)
                return Result.Failure<AuthResponse>(new Error("Auth.RolesFetchFailed", "Failed to fetch user roles", 0));

            // 4. Generate JWT token , i can use mapster here
            var tokenRequest = new TokenGenerationRequest
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles,
                Permissions = permissions
            };

            var (token, expiresIn) = _jwtTokenService.GenerateToken(tokenRequest);

            // 5. Create response (TODO: Add refresh token logic)
            var refreshToken = GenerateRefreshToken();
            var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

            user.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                ExpiresOn = refreshTokenExpiration,
                CreatedOn = DateTime.UtcNow
            });

            await _userRepository.UpdateAsync(user, cancellationToken);
            //6. return response
            var response = new AuthResponse(
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                token,
                expiresIn,
                refreshToken, // TODO: Generate actual refresh token
                refreshTokenExpiration
                );

            return Result.Success(response);
        }

        public async Task<Result<AuthResponse>> GetRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
        {
            // 1. Validate the provided JWT token (expired is OK here)
            var userId = _jwtTokenService.ValidateToken(token);
            if (userId is null)
                return Result.Failure<AuthResponse>(new Error("Auth.InvalidToken", "Invalid token", 0));

            // 2. Find user and validate refresh token
            var user = await _userRepository.FindByIdAsync(userId, cancellationToken);
            if (user is null)
                return Result.Failure<AuthResponse>(new Error("Auth.UserNotFound", "User not found", 0));

            var storedRefreshToken = user.RefreshTokens.FirstOrDefault(rt => rt.Token == refreshToken);
            if (storedRefreshToken is null || !storedRefreshToken.IsActive)
                return Result.Failure<AuthResponse>(new Error("Auth.InvalidRefreshToken", "Invalid refresh token", 0));

            // 3. Revoke old refresh token
            storedRefreshToken.RevokedOn = DateTime.UtcNow;

            // 4. Generate new tokens
            var (rolesSuccess, roles, permissions) = await _userRepository.GetUserRolesAndPermissionsAsync(user.Id, cancellationToken);

            var tokenRequest = new TokenGenerationRequest
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles,
                Permissions = permissions
            };

            var (newToken, expiresIn) = _jwtTokenService.GenerateToken(tokenRequest);
            var newRefreshToken = GenerateRefreshToken(); // ✅ استخدام الـ method المحلي
            var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);
            // 5. Save new refresh token
            user.RefreshTokens.Add(new RefreshToken
            {
                Token = newRefreshToken,
                ExpiresOn = refreshTokenExpiration,
                CreatedOn = DateTime.UtcNow
            });

            await _userRepository.UpdateAsync(user, cancellationToken);

            // 6. Return response
            var response = new AuthResponse(
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                newToken,
                expiresIn,
                newRefreshToken,
                refreshTokenExpiration
            );
            return Result.Success(response);
        }

        public async Task<Result> RevokeRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
        {
            var userId = _jwtTokenService.ValidateToken(token);
            if (userId is null)
                return Result.Failure(new Error("Auth.InvalidToken", "Invalid token", 0));

            var user = await _userRepository.FindByIdAsync(userId, cancellationToken);
            if (user is null)
                return Result.Failure(new Error("Auth.UserNotFound", "User not found", 0));

            var storedRefreshToken = user.RefreshTokens.FirstOrDefault(rt => rt.Token == refreshToken);
            if (storedRefreshToken is null || !storedRefreshToken.IsActive)
                return Result.Failure(new Error("Auth.InvalidRefreshToken", "Invalid refresh token", 0));

            // Revoke the refresh token
            storedRefreshToken.RevokedOn = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user, cancellationToken);

            return Result.Success();
        }
        

        private static string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        public Task<Result> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
