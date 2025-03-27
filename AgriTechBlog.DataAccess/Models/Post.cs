using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AgriTechBlog.Data.Models
{
    public class Post
    {
        public int Id { get; set; }
        public ApplicationUser Creator { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool Published { get; set; }
        public bool Approved { get; set; }
        public ApplicationUser Approver { get; set; }
        public virtual IEnumerable<Comment> Comments { get; set; }
    }
}
