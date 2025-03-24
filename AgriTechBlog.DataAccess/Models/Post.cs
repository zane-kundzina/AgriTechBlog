using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AgriTechBlog.Data.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }
        public Blog Blog { get; set; }
        public string Title { get; set; }       
        public string Content { get; set; }
        public string ImageUrl { get; set; } 
        public int Rank { get; set; }       
        public string AuthorId { get; set; }    // string for Identity compatibility        
        public virtual ApplicationUser Author { get; set; }
        public string Category { get; set; }       
        public DateTime Created { get; set; } = DateTime.Now;
        public Post Parent { get; set; }
        public List<Comment> MainComments { get; set; } = new List<Comment>();
    }
}
