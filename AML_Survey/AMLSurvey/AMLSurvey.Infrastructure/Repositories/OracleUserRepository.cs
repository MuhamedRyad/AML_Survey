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

            await ExecuteAsync("PKG_AUTH.SP_VALIDATE_CREDENTIALS", parameters, cancellationToken);

            var (isSuccess, errorCode) = await ExecuteWithMultipleOutputAsync<int, string>(
                "PKG_AUTH.SP_VALIDATE_CREDENTIALS",
                parameters,
                "p_is_success",
                "p_error_code",
                cancellationToken
            );

            return (isSuccess == 1, errorCode);
        }

        public async Task<(bool Success, IEnumerable<string> Roles, IEnumerable<string> Permissions)> GetUserRolesAndPermissionsAsync(
            string userId,
            CancellationToken cancellationToken = default)
        {
            var parameters = CreateParametersBuilder()
                .AddStringInput("p_user_id", userId)
                .AddRefCursor("p_cursor")
                .Build();

            var results = await ExecuteQueryAsync<dynamic>(
                "PKG_AUTH.SP_GET_USER_ROLES_PERMISSIONS",
                parameters,
                cancellationToken
            );

            var roles = results.Where(r => !string.IsNullOrEmpty(r.ROLE_NAME))
                              .Select(r => (string)r.ROLE_NAME)
                              .Distinct()
                              .ToList();

            var permissions = results.Where(r => !string.IsNullOrEmpty(r.PERMISSION_NAME))
                                   .Select(r => (string)r.PERMISSION_NAME)
                                   .Distinct()
                                   .ToList();

            return (true, roles, permissions);
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
            var parameters = CreateParametersBuilder()
                .AddStringInput("p_user_id", user.Id)
                .AddStringInput("p_first_name", user.FirstName)
                .AddStringInput("p_last_name", user.LastName)
                .AddBooleanInput("p_is_disabled", user.IsDisabled)
                .AddOutput("p_is_success", OracleMappingType.Int32)
                .AddOutput("p_error_message", OracleMappingType.Varchar2, 500)
                .Build();

            // Handle refresh tokens if any
            if (user.RefreshTokens?.Any() == true)
            {
                var newTokens = user.RefreshTokens.Where(rt => string.IsNullOrEmpty(rt.Id)).ToList();
                foreach (var token in newTokens)
                {
                    await AddRefreshTokenAsync(user.Id, token, cancellationToken);
                }

                var revokedTokens = user.RefreshTokens.Where(rt => rt.RevokedOn.HasValue).ToList();
                foreach (var token in revokedTokens)
                {
                    await RevokeRefreshTokenAsync(token.Token, cancellationToken);
                }
            }

            await ExecuteAsync("PKG_AUTH.SP_UPDATE_USER", parameters, cancellationToken);

            var isSuccess = parameters.Get<int>("p_is_success");
            var errorMessage = parameters.Get<string>("p_error_message");

            if (isSuccess == 1)
                return Result.Success();

            return Result.Failure(new Error("User.UpdateFailed", errorMessage ?? "Failed to update user", 0));
        }

        private async Task AddRefreshTokenAsync(string userId, RefreshToken token, CancellationToken cancellationToken = default)
        {
            var parameters = CreateParametersBuilder()
                .AddRefreshTokenInputs(userId, token.Token, token.ExpiresOn, token.CreatedOn)
                .Build();

            await ExecuteAsync("PKG_AUTH.SP_ADD_REFRESH_TOKEN", parameters, cancellationToken);
        }

        private async Task RevokeRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            var parameters = CreateParametersBuilder()
                .AddStringInput("p_token", token)
                .AddDateInput("p_revoked_on", DateTime.UtcNow)
                .Build();

            await ExecuteAsync("PKG_AUTH.SP_REVOKE_REFRESH_TOKEN", parameters, cancellationToken);
        }

        // Placeholder implementations
        public Task<bool> CheckPasswordAsync(User user, string password, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException("Use ValidateCredentialsAsync instead");
        }

        public Task<IEnumerable<string>> GetRolesAsync(User user, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException("Use GetUserRolesAndPermissionsAsync instead");
        }

        public Task<bool> IsLockedOutAsync(User user, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException("Handled in ValidateCredentialsAsync");
        }
    }
}