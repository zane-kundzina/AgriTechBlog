using AgriTechBlog.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AgriTechBlog.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser> 
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) {}
        
        public DbSet<Post> Posts { get; set; }

        public DbSet<Comment> Comments { get; set; }

        public DbSet<PostReaction> PostReactions { get; set; }

    }
}
