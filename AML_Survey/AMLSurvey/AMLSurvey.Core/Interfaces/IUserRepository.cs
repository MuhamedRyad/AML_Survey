using AMLSurvey.Core.Abstractions;
using AMLSurvey.Core.Entities;


namespace AMLSurvey.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<User?> FindByIdAsync(string userId, CancellationToken cancellationToken = default);
        Task<bool> CheckPasswordAsync(User user, string password, CancellationToken cancellationToken = default);
        Task<IEnumerable<string>> GetRolesAsync(User user, CancellationToken cancellationToken = default);
        Task<Result> CreateAsync(User user, string password, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(User user, CancellationToken cancellationToken = default);
        Task<bool> IsLockedOutAsync(User user, CancellationToken cancellationToken = default);

        Task<(bool Success, string? ErrorCode)> ValidateCredentialsAsync(string email, string password, bool lockoutOnFailure = false, CancellationToken cancellationToken = default);
        Task<(bool Success, IEnumerable<string> Roles, IEnumerable<string> Permissions)> GetUserRolesAndPermissionsAsync(string userId, CancellationToken cancellationToken = default);

      /*
       Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
        Task<string> GenerateEmailConfirmationTokenAsync(string userId, CancellationToken cancellationToken = default);
        Task<(bool Success, string? ErrorCode)> ConfirmEmailAsync(string userId, string token, CancellationToken cancellationToken = default);
      */
    }
}