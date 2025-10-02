using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace AMLSurvey.Infrastructure.Identity
{
    public class ApplicationRole : IdentityRole
    {
        //public string Description { get; set; } = string.Empty;
        public ApplicationRole()
        {
            Id = Guid.CreateVersion7().ToString();//
        }

        public bool IsDefault { get; set; }
        public bool IsDeleted { get; set; }
    }
}
