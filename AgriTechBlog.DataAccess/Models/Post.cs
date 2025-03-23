using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AgriTechBlog.Data.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }
        public string ImageUrl { get; set; } 
        public int Rank { get; set; }
        [Required]
        public string AuthorId { get; set; }    // string for Identity compatibility
        [Required]
        public virtual ApplicationUser Author { get; set; }
        public string Category { get; set; }
        [Required]
        public DateTime Created { get; set; } = DateTime.Now;
        public List<Comment> MainComments { get; set; } = new List<Comment>();
    }
}
