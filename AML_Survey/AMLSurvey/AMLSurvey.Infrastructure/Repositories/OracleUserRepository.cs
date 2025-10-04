using AMLSurvey.Core.Abstractions;
using AMLSurvey.Core.Entities;
using AMLSurvey.Core.Interfaces;
using AMLSurvey.Infrastructure.Repositories.Base;
using AMLSurvey.Infrastructure.Helpers;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace AMLSurvey.Infrastructure.Repositories
{
    public class OracleUserRepository : BaseOracleRepository, IUserRepository
    {
        public OracleUserRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var parameters = CreateParametersBuilder()
                .AddStringInput("p_email", email)
                .AddRefCursor("p_cursor")
                .Build();

            var users = await ExecuteQueryAsync<User>(
                "PKG_AUTH.SP_GET_USER_BY_EMAIL",
                parameters,
                cancellationToken
            );

            return users.FirstOrDefault();
        }

        public async Task<User?> FindByIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            var parameters = CreateParametersBuilder()
                .AddStringInput("p_user_id", userId)
                .AddRefCursor("p_cursor")
                .Build();

            var users = await ExecuteQueryAsync<User>(
                "PKG_AUTH.SP_GET_USER_BY_ID",
                parameters,
                cancellationToken
            );

            return users.FirstOrDefault();
        }

        public async Task<(bool Success, string? ErrorCode)> ValidateCredentialsAsync(
            string email,
            string password,
            bool lockoutOnFailure = false,
            CancellationToken cancellationToken = default)
        {
            var parameters = CreateParametersBuilder()
                .AddUserAuthInputs(email, password, lockoutOnFailure)
                .AddAuthOutputs()
                .Build();

            // ✅ ناديها مرة واحدة بس!
            await ExecuteAsync("PKG_AUTH.SP_VALIDATE_CREDENTIALS", parameters, cancellationToken);

            // ✅ اقرا الـ Output Parameters
            var isSuccess = parameters.Get<int>("p_is_success");
            var errorCode = parameters.Get<string>("p_error_code");

            return (isSuccess == 1, errorCode);
        }

        public async Task<(bool Success, IEnumerable<string> Roles, IEnumerable<string> Permissions)> GetUserRolesAndPermissionsAsync(
            string userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var parameters = CreateParametersBuilder()
                    .AddStringInput("p_user_id", userId)
                    .AddRefCursor("p_cursor")
                    .Build();

                var results = await ExecuteQueryAsync<UserRolePermissionDto>(
                    "PKG_AUTH.SP_GET_USER_ROLES_PERMISSIONS",
                    parameters,
                    cancellationToken
                );

                if (!results.Any())
                    return (false, Enumerable.Empty<string>(), Enumerable.Empty<string>());

                var roles = results
                    .Where(r => !string.IsNullOrEmpty(r.RoleName))
                    .Select(r => r.RoleName!)
                    .Distinct()
                    .ToList();

                var permissions = results
                    .Where(r => !string.IsNullOrEmpty(r.PermissionName))
                    .Select(r => r.PermissionName!)
                    .Distinct()
                    .ToList();

                return (true, roles, permissions);
            }
            catch
            {
                return (false, Enumerable.Empty<string>(), Enumerable.Empty<string>());
            }
        }

        public async Task<Result> CreateAsync(User user, string password, CancellationToken cancellationToken = default)
        {
            var parameters = CreateParametersBuilder()
                .AddUserCreateInputs(user.Email, user.FirstName, user.LastName, password)
                .AddUserCreateOutputs()
                .Build();

            await ExecuteAsync("PKG_AUTH.SP_CREATE_USER", parameters, cancellationToken);

            var isSuccess = parameters.Get<int>("p_is_success");
            var errorMessage = parameters.Get<string>("p_error_message");
            var userId = parameters.Get<string>("p_user_id");

            if (isSuccess == 1)
            {
                user.Id = userId!;
                return Result.Success();
            }

            return Result.Failure(new Error("User.CreationFailed", errorMessage ?? "Failed to create user", 0));
        }

        public async Task<Result> UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            // ✅ 1. Update User Basic Info
            var parameters = CreateParametersBuilder()
                .AddStringInput("p_user_id", user.Id)
                .AddStringInput("p_first_name", user.FirstName)
                .AddStringInput("p_last_name", user.LastName)
                .AddBooleanInput("p_is_disabled", user.IsDisabled)
                .AddBooleanInput("p_email_confirmed", user.EmailConfirmed)
                .AddIntOutput("p_is_success")          // ✅ استخدم AddIntOutput
                .AddStringOutput("p_error_message", 500) // ✅ استخدم AddStringOutput
                .Build();

            await ExecuteAsync("PKG_AUTH.SP_UPDATE_USER", parameters, cancellationToken);

            var isSuccess = parameters.Get<int>("p_is_success");
            var errorMessage = parameters.Get<string>("p_error_message");

            if (isSuccess != 1)
                return Result.Failure(new Error("User.UpdateFailed", errorMessage ?? "Failed to update user", 0));

            // ✅ 2. Handle Refresh Tokens
            if (user.RefreshTokens?.Any() == true)
            {
                // Add new tokens (اللي معندهاش RevokedOn)
                var newTokens = user.RefreshTokens
                    .Where(rt => !rt.RevokedOn.HasValue)
                    .ToList();

                foreach (var token in newTokens)
                {
                    await AddRefreshTokenAsync(user.Id, token, cancellationToken);
                }

                // Revoke old tokens (اللي معاها RevokedOn)
                var revokedTokens = user.RefreshTokens
                    .Where(rt => rt.RevokedOn.HasValue)
                    .ToList();

                foreach (var token in revokedTokens)
                {
                    await RevokeRefreshTokenAsync(token.Token, token.RevokedOn!.Value, cancellationToken);
                }
            }

            return Result.Success();
        }

        private async Task AddRefreshTokenAsync(string userId, RefreshToken token, CancellationToken cancellationToken = default)
        {
            var parameters = CreateParametersBuilder()
                .AddRefreshTokenInputs(userId, token.Token, token.ExpiresOn, token.CreatedOn)
                .AddStringInput("p_created_by_ip", "token.CreatedByIp") // ✅ إضافة IP   ؟؟؟؟؟
                .Build();

            await ExecuteAsync("PKG_AUTH.SP_ADD_REFRESH_TOKEN", parameters, cancellationToken);
        }

        private async Task RevokeRefreshTokenAsync(string token, DateTime revokedOn, CancellationToken cancellationToken = default)
        {
            var parameters = CreateParametersBuilder()
                .AddStringInput("p_token", token)
                .AddDateInput("p_revoked_on", revokedOn)
                .Build();

            await ExecuteAsync("PKG_AUTH.SP_REVOKE_REFRESH_TOKEN", parameters, cancellationToken);
        }

        public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        {
            var parameters = CreateParametersBuilder()
                .AddStringInput("p_email", email)
                .AddIntOutput("p_exists")
                .Build();

            await ExecuteAsync("PKG_AUTH.SP_CHECK_EMAIL_EXISTS", parameters, cancellationToken);

            var exists = parameters.Get<int>("p_exists");
            return exists == 1;
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(string userId, CancellationToken cancellationToken = default)
        {
            var parameters = CreateParametersBuilder()
                .AddStringInput("p_user_id", userId)
                .AddStringOutput("p_token", 500)
                .Build();

            await ExecuteAsync("PKG_AUTH.SP_GENERATE_EMAIL_TOKEN", parameters, cancellationToken);

            return parameters.Get<string>("p_token") ?? string.Empty;
        }

        public async Task<(bool Success, string? ErrorCode)> ConfirmEmailAsync(string userId, string token, CancellationToken cancellationToken = default)
        {
            var parameters = CreateParametersBuilder()
                .AddStringInput("p_user_id", userId)
                .AddStringInput("p_token", token)
                .AddIntOutput("p_is_success")
                .AddStringOutput("p_error_code", 50)
                .Build();

            await ExecuteAsync("PKG_AUTH.SP_CONFIRM_EMAIL", parameters, cancellationToken);

            var isSuccess = parameters.Get<int>("p_is_success");
            var errorCode = parameters.Get<string>("p_error_code");

            return (isSuccess == 1, errorCode);
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            var parameters = CreateParametersBuilder()
                .AddStringInput("p_token", token)
                .AddRefCursor("p_cursor")
                .Build();

            var tokens = await ExecuteQueryAsync<RefreshToken>(
                "PKG_AUTH.SP_GET_REFRESH_TOKEN",
                parameters,
                cancellationToken
            );

            return tokens.FirstOrDefault();
        }

        // ✅ Deprecated Methods (بدل NotImplementedException)
        [Obsolete("Use ValidateCredentialsAsync instead")]
        public Task<bool> CheckPasswordAsync(User user, string password, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("This method is deprecated. Use ValidateCredentialsAsync instead.");
        }

        [Obsolete("Use GetUserRolesAndPermissionsAsync instead")]
        public Task<IEnumerable<string>> GetRolesAsync(User user, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("This method is deprecated. Use GetUserRolesAndPermissionsAsync instead.");
        }

        [Obsolete("Handled in ValidateCredentialsAsync")]
        public Task<bool> IsLockedOutAsync(User user, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Lockout is handled in ValidateCredentialsAsync.");
        }
    }

    // ✅ DTO للـ Roles & Permissions
    internal class UserRolePermissionDto
    {
        public string? RoleName { get; set; }
        public string? PermissionName { get; set; }
    }
}