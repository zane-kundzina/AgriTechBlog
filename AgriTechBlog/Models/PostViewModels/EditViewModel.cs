using AgriTechBlog.Data.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace AgriTechBlog.WebUI.Models.PostViewModels
{
    [ExcludeFromCodeCoverage]

    public class EditViewModel
    {
        [Display(Name = "Header Image")]
        public IFormFile HeaderImage { get; set; }
        public Post Post { get; set; }
    }
}
