using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMLSurvey.Core.DTOS.Auth
{
    public record AuthResponse(
        // user info
        string Id,
        string? Email,
        string FirstName,
        string LastName,

        //token info
        string Token,
        int ExpiresIn,
        string RefreshToken,
        DateTime RefreshTokenExpiration
    );
}
