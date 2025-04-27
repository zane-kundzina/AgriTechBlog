using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace AgriTechBlog.Data.Models
{
    [ExcludeFromCodeCoverage]
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        public int PostId { get; set; }
        public Post Post { get; set; }

        public string AuthorId { get; set; }
        public ApplicationUser Author { get; set; }
       
        public string Content { get; set; }

        public int? ParentId { get; set; }
        public Comment Parent { get; set; }
        
        public DateTime CreatedOn { get; set; }

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>(); // child replies
        public bool IsHidden { get; set; } = false;
    }
}
