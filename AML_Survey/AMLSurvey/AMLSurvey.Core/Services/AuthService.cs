using AMLSurvey.Core.Abstractions;
using AMLSurvey.Core.DTOS.Auth;
using AMLSurvey.Core.Entities;
using AMLSurvey.Core.Interfaces;
using System.Security.Cryptography;

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
            // 1. Validate credentials using Oracle stored procedure
            var (success, errorCode) = await _userRepository.ValidateCredentialsAsync(email, password, true, cancellationToken);

            if (!success)
            {
                var error = errorCode switch
                {
                    "DisabledUser" => new Error("Auth.DisabledUser", "User account is disabled", 401),
                    "EmailNotConfirmed" => new Error("Auth.EmailNotConfirmed", "Email not confirmed", 401),
                    "LockedUser" => new Error("Auth.LockedUser", "User account is locked", 423),
                    _ => new Error("Auth.InvalidCredentials", "Invalid email or password", 401)
                };
                return Result.Failure<AuthResponse>(error);
            }

            // 2. Get user details from Oracle
            var user = await _userRepository.FindByEmailAsync(email, cancellationToken);
            if (user is null)
                return Result.Failure<AuthResponse>(new Error("Auth.UserNotFound", "User not found", 404));

            // 3. Get roles and permissions from Oracle
            var (rolesSuccess, roles, permissions) = await _userRepository.GetUserRolesAndPermissionsAsync(user.Id, cancellationToken);

            if (!rolesSuccess)
                return Result.Failure<AuthResponse>(new Error("Auth.RolesFetchFailed", "Failed to fetch user roles", 500));

            // 4. Generate JWT token
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

            // 5. Generate and save refresh token to Oracle
            var refreshToken = GenerateRefreshToken();
            var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

            user.RefreshTokens ??= new List<RefreshToken>();
            user.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
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
                token,
                expiresIn,
                refreshToken,
                refreshTokenExpiration
            );

            return Result.Success(response);
        }

        public async Task<Result<AuthResponse>> GetRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
        {
            // 1. Validate JWT token (expired is OK)
            var userId = _jwtTokenService.ValidateToken(token);
            if (userId is null)
                return Result.Failure<AuthResponse>(new Error("Auth.InvalidToken", "Invalid token", 401));

            // 2. Find user and validate refresh token from Oracle
            var user = await _userRepository.FindByIdAsync(userId, cancellationToken);
            if (user is null)
                return Result.Failure<AuthResponse>(new Error("Auth.UserNotFound", "User not found", 404));

            // Note: In Oracle implementation, refresh token validation should be done via stored procedure
            // For now, we'll keep the logic here, but ideally move to Oracle
            var storedRefreshToken = user.RefreshTokens?.FirstOrDefault(rt => rt.Token == refreshToken);
            if (storedRefreshToken is null || !storedRefreshToken.IsActive)
                return Result.Failure<AuthResponse>(new Error("Auth.InvalidRefreshToken", "Invalid refresh token", 401));

            // 3. Revoke old refresh token
            storedRefreshToken.RevokedOn = DateTime.UtcNow;

            // 4. Get fresh roles and permissions
            var (rolesSuccess, roles, permissions) = await _userRepository.GetUserRolesAndPermissionsAsync(user.Id, cancellationToken);

            if (!rolesSuccess)
                return Result.Failure<AuthResponse>(new Error("Auth.RolesFetchFailed", "Failed to fetch user roles", 500));

            // 5. Generate new tokens
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
            var newRefreshToken = GenerateRefreshToken();
            var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

            // 6. Save new refresh token to Oracle
            user.RefreshTokens.Add(new RefreshToken
            {
                Token = newRefreshToken,
                ExpiresOn = refreshTokenExpiration,
                CreatedOn = DateTime.UtcNow
            });

            await _userRepository.UpdateAsync(user, cancellationToken);

            // 7. Return response
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
                return Result.Failure(new Error("Auth.InvalidToken", "Invalid token", 401));

            var user = await _userRepository.FindByIdAsync(userId, cancellationToken);
            if (user is null)
                return Result.Failure(new Error("Auth.UserNotFound", "User not found", 404));

            var storedRefreshToken = user.RefreshTokens?.FirstOrDefault(rt => rt.Token == refreshToken);
            if (storedRefreshToken is null || !storedRefreshToken.IsActive)
                return Result.Failure(new Error("Auth.InvalidRefreshToken", "Invalid refresh token", 401));

            // Revoke the refresh token
            storedRefreshToken.RevokedOn = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user, cancellationToken);

            return Result.Success();
        }
        

        private static string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        public async Task<Result> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            //
            var user = new User
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                EmailConfirmed = false,
                IsDisabled = false,
                CreatedAt = DateTime.UtcNow
            };

            return await _userRepository.CreateAsync(user, request.Password, cancellationToken);
        }
    }
}
