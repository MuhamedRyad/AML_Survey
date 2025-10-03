using AMLSurvey.Core.Abstractions;
using AMLSurvey.Core.Interfaces;
using AMLSurvey.Core.Entities;
using AMLSurvey.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Mapster;

namespace AMLSurvey.Infrastructure.Repositories
{
    public class IdentityUserRepository  : IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IdentityUserRepository (
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var applicationUser = await _userManager.FindByEmailAsync(email);
            return applicationUser?.Adapt<User>(); // Mapster conversion
        }

        public async Task<User?> FindByIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            var applicationUser = await _userManager.FindByIdAsync(userId);
            return applicationUser?.Adapt<User>();
        }

        public async Task<bool> CheckPasswordAsync(User user, string password, CancellationToken cancellationToken = default)
        {
            var applicationUser = await _userManager.FindByIdAsync(user.Id);
            if (applicationUser is null) return false;

            var result = await _signInManager.CheckPasswordSignInAsync(applicationUser, password, false);
            return result.Succeeded;
        }

        public async Task<IEnumerable<string>> GetRolesAsync(User user, CancellationToken cancellationToken = default)
        {
            var applicationUser = await _userManager.FindByIdAsync(user.Id);
            if (applicationUser is null) return [];

            return await _userManager.GetRolesAsync(applicationUser);
        }

        public async Task<Result> CreateAsync(User user, string password, CancellationToken cancellationToken = default)
        {
            var applicationUser = user.Adapt<ApplicationUser>();
            var result = await _userManager.CreateAsync(applicationUser, password);

            if (result.Succeeded)
                return Result.Success();

            var errors = result.Errors.Select(e => e.Description);
            return Result.Failure(new Error( "User.CreationFailed",string.Join(", ", errors),0));
        }

        public async Task<Result> UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            var applicationUser = await _userManager.FindByIdAsync(user.Id);
            if (applicationUser is null)
                return Result.Failure(new Error("User.NotFound", "User not found",0));

            // Update properties
            applicationUser.FirstName = user.FirstName;
            applicationUser.LastName = user.LastName;
            applicationUser.IsDisabled = user.IsDisabled;
            // Update RefreshTokens if needed

            var result = await _userManager.UpdateAsync(applicationUser);
            
            if (result.Succeeded)
                return Result.Success();

            var errors = result.Errors.Select(e => e.Description);
            return Result.Failure(new Error("User.UpdateFailed", string.Join(", ", errors), 0));
        }

        public async Task<bool> IsLockedOutAsync(User user, CancellationToken cancellationToken = default)
        {
            var applicationUser = await _userManager.FindByIdAsync(user.Id);
            if (applicationUser is null) return false;

            return await _userManager.IsLockedOutAsync(applicationUser);
        }

        public async Task<(bool Success, string? ErrorCode)> ValidateCredentialsAsync(string email, string password, bool lockoutOnFailure = false,
            CancellationToken cancellationToken = default)
        {
            var appUser = await _userManager.FindByEmailAsync(email);

            if (appUser is null)
                return (false, "InvalidCredentials");

            if (appUser.IsDisabled)
                return (false, "DisabledUser");

            var result = await _signInManager.PasswordSignInAsync(appUser, password, false, lockoutOnFailure);

            if (result.Succeeded)
                return (true, null);

            var errorCode = result.IsNotAllowed ? "EmailNotConfirmed"
                : result.IsLockedOut ? "LockedUser"
                : "InvalidCredentials";

            return (false, errorCode);
        }

     

        public async Task<(bool Success, IEnumerable<string> Roles, IEnumerable<string> Permissions)> GetUserRolesAndPermissionsAsync(string userId, CancellationToken cancellationToken = default)
        {
           /* var appUser = await _userManager.FindByIdAsync(userId);

            if (appUser is null)
                return (false, Enumerable.Empty<string>(), Enumerable.Empty<string>());

            var roles = await _userManager.GetRolesAsync(appUser);

            var permissions = await(from r in _context.Roles
                    join p in _context.RoleClaims on r.Id equals p.RoleId
                    where roles.Contains(r.Name!)
                    select p.ClaimValue!)
                .Distinct()
                .ToListAsync(cancellationToken);*/

            return (true, [], []); //roles, permissions);
        }

      
    }
}