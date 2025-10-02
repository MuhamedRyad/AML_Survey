using AMLSurvey.Core.Models;
using AMLSurvey.Infrastructure.Config;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AMLSurvey.Infrastructure.Identity
{
    public class ApplicationContext : IdentityDbContext<ApplicationUser,ApplicationRole,string>
    { 
        private readonly IHttpContextAccessor _httpContextAccessor;   // if want to use UserId in SaveChanges

        //DbContextOptions  => Configuration في Startup
        public ApplicationContext(DbContextOptions<ApplicationContext> options,
            IHttpContextAccessor contextAccessor
        ) : base(options)
        {
            _httpContextAccessor = contextAccessor;
        }

        // public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
           
            builder.ApplyConfiguration(new UserConfiguration());
           // builder.ApplyConfiguration(new RoleConfiguration());
            //builder.ApplyConfiguration(new UserRoleConfiguration());
        }



        // تسجيل من قام بإنشاء أو تعديل البيانات ومتى حدث ذلك
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var userId = _httpContextAccessor.HttpContext?.User?
                .FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";

            var entries = ChangeTracker
                .Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            { // ✅ prop in AuditableEntity(CreatedOn,CreatedById)
                if (entry.Entity is AuditableEntity auditableEntity)
                {
                    if (entry.State == EntityState.Added)
                    {
                        auditableEntity.CreatedOn = DateTime.UtcNow;
                        auditableEntity.CreatedById = userId;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        auditableEntity.UpdatedOn = DateTime.UtcNow;
                        auditableEntity.UpdatedById = userId;
                    }
                }
                else // ✅ Shadow Properties or run tim prop
                {
                    if (entry.State == EntityState.Added)
                    {
                        entry.Property("CreatedOn").CurrentValue = DateTime.UtcNow;
                        entry.Property("CreatedById").CurrentValue = userId;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        entry.Property("UpdatedOn").CurrentValue = DateTime.UtcNow;
                        entry.Property("UpdatedById").CurrentValue = userId;
                    }
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }


    }
}
