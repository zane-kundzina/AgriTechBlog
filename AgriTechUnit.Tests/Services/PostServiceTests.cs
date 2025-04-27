using AgriTechBlog.Data.Models;
using AgriTechBlog.Data;
using AgriTechBlog.Service;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Linq;

namespace AgriTechUnit.Tests.Services
{
    public class PostServiceTests
    {
        private DbContextOptions<ApplicationDbContext> _options;

        public PostServiceTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // ensures isolation
                .Options;
        }

        private DbContextOptions<ApplicationDbContext> CreateNewContextOptions()
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }


        [Fact]
        public void GetPost_ShouldReturnPostWithIncludes()
        {
            using var context = new ApplicationDbContext(_options);

            var post = new Post
            {
                Id = 1,
                Title = "Test Post",
                Creator = new ApplicationUser { Id = "1", UserName = "User1" },
                Reactions = new List<PostReaction>(),
                Comments = new List<Comment>()
            };
            context.Posts.Add(post);
            context.SaveChanges();

            var service = new PostService(context);
            var result = service.GetPost(1);

            Assert.NotNull(result);
            Assert.Equal("Test Post", result.Title);
        }

        [Fact]
        public void GetPost_ShouldReturnCorrectPost()
        {
            // Arrange
            var options = CreateNewContextOptions();
            using var context = new ApplicationDbContext(options);
            var post = new Post
            {
                Id = 1,
                Title = "Test Post",
                Creator = new ApplicationUser { Id = "user1", UserName = "TestUser" },
                Reactions = new List<PostReaction>(),
                Comments = new List<Comment>()
            };
            context.Posts.Add(post);
            context.SaveChanges();

            var service = new PostService(context);

            // Act
            var result = service.GetPost(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Post", result.Title);
            Assert.Equal("TestUser", result.Creator.UserName);
        }

        [Fact]
        public void GetPosts_WithSearchString_ShouldReturnMatchingPosts()
        {
            // Arrange
            var options = CreateNewContextOptions();
            using var context = new ApplicationDbContext(options);
            context.Posts.AddRange(
                new Post { Title = "First Post", Content = "Something about agriculture" },
                new Post { Title = "Second Post", Content = "Farming tips" }
            );
            context.SaveChanges();

            var service = new PostService(context);

            // Act
            var result = service.GetPosts("agriculture").ToList();

            // Assert
            Assert.Single(result);
            Assert.Contains("First Post", result[0].Title);
        }

        [Fact]
        public void GetPosts_ByUser_ShouldReturnUserPosts()
        {
            // Arrange
            var options = CreateNewContextOptions();
            using var context = new ApplicationDbContext(options);
            var user = new ApplicationUser { Id = "user1" };
            var post = new Post { Title = "User's Post", Creator = user };
            context.Posts.Add(post);
            context.SaveChanges();

            var service = new PostService(context);

            // Act
            var result = service.GetPosts(user).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("User's Post", result[0].Title);
        }

        [Fact]
        public void GetComment_ShouldReturnCorrectComment()
        {
            // Arrange
            var options = CreateNewContextOptions();
            using var context = new ApplicationDbContext(options);
            var comment = new Comment
            {
                Id = 1,
                Content = "Test Comment",
                Author = new ApplicationUser { Id = "author1" },
                Post = new Post { Id = 1, Title = "Test Post" }
            };
            context.Comments.Add(comment);
            context.SaveChanges();

            var service = new PostService(context);

            // Act
            var result = service.GetComment(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Comment", result.Content);
        }

        [Fact]
        public async Task Add_Post_ShouldAddPost()
        {
            // Arrange
            var options = CreateNewContextOptions();
            using var context = new ApplicationDbContext(options);
            var post = new Post { Title = "New Post" };
            var service = new PostService(context);

            // Act
            var result = await service.Add(post);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Post", result.Title);
            Assert.Equal(1, context.Posts.Count());
        }

        [Fact]
        public async Task Add_Comment_ShouldAddComment()
        {
            // Arrange
            var options = CreateNewContextOptions();
            using var context = new ApplicationDbContext(options);
            var comment = new Comment { Content = "New Comment" };
            var service = new PostService(context);

            // Act
            var result = await service.Add(comment);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Comment", result.Content);
            Assert.Equal(1, context.Comments.Count());
        }

        [Fact]
        public async Task Update_Post_ShouldUpdatePost()
        {
            // Arrange
            var options = CreateNewContextOptions();
            using var context = new ApplicationDbContext(options);
            var post = new Post { Title = "Old Title" };
            context.Posts.Add(post);
            context.SaveChanges();

            var service = new PostService(context);

            post.Title = "Updated Title";

            // Act
            var result = await service.Update(post);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Title", result.Title);
        }
    }
}
