using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AgriTechBlog.Data.Models
{
    public class ApplicationUser : IdentityUser
    { 
        [Required]
        public string Role { get; set; } // Admin, Writer, Reader
    }
}
