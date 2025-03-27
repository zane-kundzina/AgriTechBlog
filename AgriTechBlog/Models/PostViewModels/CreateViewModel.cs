using AgriTechBlog.Data.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AgriTechBlog.WebUI.Models.PostViewModels
{
    public class CreateViewModel
    {
        [Required, Display(Name ="Header Image")]
        public IFormFile HeaderImage { get; set; }
        public Post Post { get; set; }
    }
}
