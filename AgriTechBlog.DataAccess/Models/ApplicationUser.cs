using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace AgriTechBlog.Data.Models
{
    [ExcludeFromCodeCoverage]

    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        public string FirstName { get; set; }
        [PersonalData]
        public string LastName { get; set; }

        public string SubHeader { get; set; }

        public string AboutContent { get; set; }
       
        public string Role { get; set; } // Admin, Writer, Reader
    }
}
