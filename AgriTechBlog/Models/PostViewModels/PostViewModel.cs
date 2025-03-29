using AgriTechBlog.Data.Models;

namespace AgriTechBlog.WebUI.Models.PostViewModels
{
    public class PostViewModel
    {
        public Post Post { get; set; }

        public Comment Comment { get; set; }
    }
}
