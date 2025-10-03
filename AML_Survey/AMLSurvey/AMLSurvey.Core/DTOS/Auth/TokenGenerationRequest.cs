using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMLSurvey.Core.DTOS.Auth
{
    public record TokenGenerationRequest
    {
        public string UserId { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;

        public IEnumerable<string> Roles { get; init; } = [];
        public IEnumerable<string> Permissions { get; init; } = [];

    }
}
