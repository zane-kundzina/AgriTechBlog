using AgriTechBlog.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AgriTechBlog.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser> 
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) {}
        
        public DbSet<Blog> Blogs { get; set; }

        public DbSet<Post> Posts { get; set; }

        //public DbSet<Comment> Comments { get; set; }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder); // Ensure IdentityDbContext configurations are applied

        //    // Self-referencing Comment (nested replies)
        //    modelBuilder.Entity<Comment>()
        //        .HasOne(c => c.MainComment)
        //        .WithMany(c => c.Replies)
        //        .HasForeignKey(c => c.MainCommentId)
        //        .OnDelete(DeleteBehavior.Restrict);  // Prevent cyclic cascade delete

        //    // User-Post relationship
        //    modelBuilder.Entity<Post>()
        //        .HasOne(p => p.Author)
        //        .WithMany()  // User can have many posts
        //        .HasForeignKey(p => p.AuthorId)
        //        .OnDelete(DeleteBehavior.Cascade);

        //    // User-Comment relationship
        //    modelBuilder.Entity<Comment>()
        //        .HasOne(c => c.Author)
        //        .WithMany()  // User can have many comments
        //        .HasForeignKey(c => c.AuthorId)
        //        .OnDelete(DeleteBehavior.Cascade);
        //}
    }
}
