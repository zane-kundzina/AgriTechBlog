using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace AgriTechBlog.Data.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }
        public string Message { get; set; }
        public DateTime Created {  get; set; }
        public int PostId { get; set; }
        public virtual Post Post { get; set; }
        public string AuthorId { get; set; }    // string for Identity compatibility
        public virtual ApplicationUser Author { get; set; }
        public bool IsReported { get; set; } = false;
        public bool IsDeleted { get; set; } = false;

        // Self-referencing relationship for nesting comments
        public int? MainCommentId { get; set; }  
        public virtual Comment MainComment { get; set; }

        public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>()
            .OrderBy(c => c.Created).ToList();
    }
}
