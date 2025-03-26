using AgriTechBlog.Data.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AgriTechBlog.WebUI.Models.BlogViewModels
{
    public class CreateViewModel
    {
        [Required, Display(Name ="Header Image")]
        public IFormFile BlogHeaderImage { get; set; }
        public Blog Blog { get; set; }
    }
}
