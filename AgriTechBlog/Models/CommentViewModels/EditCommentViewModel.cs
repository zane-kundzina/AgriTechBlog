using System.ComponentModel.DataAnnotations;

namespace AgriTechBlog.WebUI.Models.CommentViewModels
{
    public class EditCommentViewModel
    {
        public int CommentId { get; set; }

        [Required]
        public string Content { get; set; }
    }
}
