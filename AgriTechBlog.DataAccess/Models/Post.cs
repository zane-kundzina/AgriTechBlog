using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AgriTechBlog.Data.Models
{
    public class Post
    {
        public int Id { get; set; }
        
        public string CreatorID {  get; set; }
        public ApplicationUser Creator { get; set; }
        
        [Required]
        public string Title { get; set; }
        
        [Required]
        public string Content { get; set; }
        
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        
        public bool Published { get; set; }
        public bool Approved { get; set; }

        public string ApproverId { get; set; }
        public ApplicationUser Approver { get; set; }

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();  // changed from IEnumerable to ICollection
        public ICollection<PostReaction> Reactions { get; set; } = new List<PostReaction>();
    }
}
