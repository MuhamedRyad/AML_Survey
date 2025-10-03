using AMLSurvey.Core.DTOS.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMLSurvey.Core.Interfaces
{
    public interface IJwtTokenService
    {
        (string token, int expiresIn) GenerateToken(TokenGenerationRequest TG_Request); //appuser,IEnumerable<string> roles, IEnumerable<string> permissions
        string? ValidateToken(string token);
        
    }
}
