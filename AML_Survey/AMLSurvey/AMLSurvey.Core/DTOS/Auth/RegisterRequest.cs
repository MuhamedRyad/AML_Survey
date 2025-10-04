using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMLSurvey.Core.DTOS.Auth
{
    public record RegisterRequest(
        string Email,
        string Password,
        string FirstName,
        string LastName,
        string ConfirmPassword 

    );
}
