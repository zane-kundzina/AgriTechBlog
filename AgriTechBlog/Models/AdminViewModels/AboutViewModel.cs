using AgriTechBlog.Data.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace AgriTechBlog.WebUI.Models.AdminViewModels
{
    [ExcludeFromCodeCoverage]

    public class AboutViewModel
    {
        public ApplicationUser ApplicationUser { get; set; }
        [Display(Name = "Header Image")]
        public IFormFile HeaderImage { get; set; }
        [Display(Name = "Sub-header")]
        public string SubHeader { get; set; }
        public string Content { get; set; }
    }
}
