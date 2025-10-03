using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMLSurvey.Infrastructure.Security
{
    public class JwtOptions
    {
        public const string SectionName = "Jwt";

        [Required(ErrorMessage = "JWT Key is required")]
        [MinLength(32, ErrorMessage = "JWT Key must be at least 32 characters for HS256")]
        public string Key { get; set; } = string.Empty;

        [Required(ErrorMessage = "Issuer is required")]
        public string Issuer { get; set; } = string.Empty;

        [Required(ErrorMessage = "Audience is required")]
        public string Audience { get; set; } = string.Empty;

        [Range(1, 1440, ErrorMessage = "Expiration must be between 1 and 1440 minutes")]
        public int ExpiryMinutes { get; set; } = 60;
        [Range(1, 365, ErrorMessage = "Refresh token expiration must be between 1 and 365 days")]
        public int RefreshTokenExpirationDays { get; set; } = 7;
    }
}
