using AMLSurvey.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMLSurvey.Infrastructure.Config
{
    public class UserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
          /*  builder.OwnsMany(x => x.RefreshTokens)
                .ToTable("RefreshTokens")
                .WithOwner()
                .HasForeignKey("UserId"); 
          */  //custam key in migration

            builder.Property(x => x.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.LastName)
                .IsRequired()
                .HasMaxLength(100);

            // تحسين أداء الاستعلامات المتكررة
            builder.HasIndex(x => x.Email); //email in IdentityUser

            // تكوين فهرس مركب للبحث بالاسم
            builder.HasIndex(x => new { x.FirstName, x.LastName });
        }
    }


}
