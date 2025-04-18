using System;
using System.Collections.Generic;
using System.Text;

namespace AgriTechBlog.Data.Models
{
    public class PostReaction
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int PostId { get; set; }
        public Post Post { get; set; }        

        public bool IsLike { get; set; } // true = like, false = dislike
    }
}
