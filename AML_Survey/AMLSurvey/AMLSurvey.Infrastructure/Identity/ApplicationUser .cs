using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace AMLSurvey.Infrastructure.Identity
{
    public sealed class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            //DB column بتاع Id يفضل nvarchar(450)
            Id = Guid.CreateVersion7().ToString();

            //إلغاء أي توكنات قديمة بعد اي تعديل أمني
            SecurityStamp = Guid.CreateVersion7().ToString();
        }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsDisabled { get; set; }

        //public List<RefreshToken> RefreshTokens { get; set; } = [];
    }
}
