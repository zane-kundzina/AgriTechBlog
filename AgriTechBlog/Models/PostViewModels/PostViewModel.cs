using AgriTechBlog.Data.Models;
using System.Diagnostics.CodeAnalysis;

namespace AgriTechBlog.WebUI.Models.PostViewModels
{
    [ExcludeFromCodeCoverage]

    public class PostViewModel
    {
        public Post Post { get; set; }

        public Comment Comment { get; set; }
    }
}
