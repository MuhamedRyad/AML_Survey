using AMLSurvey.Core.Models;
using AMLSurvey.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMLSurvey.Infrastructure.Config
{
    public static class AuditableEntityConfiguration
    {
        public static void ConfigureAuditableEntity<TEntity>(
            this EntityTypeBuilder<TEntity> builder,
            bool createdByRequired = true)
            where TEntity : class
        {
            // لو الكيان بيرث من AuditableEntity → Typed Properties
            if (typeof(AuditableEntity).IsAssignableFrom(typeof(TEntity)))
            {
                builder.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(nameof(AuditableEntity.CreatedById))
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(createdByRequired);

                builder.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(nameof(AuditableEntity.UpdatedById))
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);

                builder.HasIndex(nameof(AuditableEntity.CreatedById));
                builder.HasIndex(nameof(AuditableEntity.UpdatedById));
                builder.HasIndex(nameof(AuditableEntity.CreatedOn));
            }
            else
            {
                // لو الكيان مش بيرث → Shadow Properties
                builder.Property<int?>("CreatedById");
                builder.Property<int?>("UpdatedById");
                builder.Property<DateTime>("CreatedOn");

                builder.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey("CreatedById")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(createdByRequired);

                builder.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey("UpdatedById")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);

                builder.HasIndex("CreatedById");
                builder.HasIndex("UpdatedById");
                builder.HasIndex("CreatedOn");
            }
        }
    }
}

