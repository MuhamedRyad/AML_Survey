using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMLSurvey.Core.Entities
{
    public class User
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsDisabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public bool EmailConfirmed { get; set; }
        public string FullName => $"{FirstName} {LastName}".Trim();

        public List<RefreshToken> RefreshTokens { get; set; } = new();
    }
}
