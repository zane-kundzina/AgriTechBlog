using AgriTechBlog.Data.Models;
using AgriTechBlog.Service.Interfaces;
using AgriTechBlog.WebUI.BusinessManagers;
using AgriTechBlog.WebUI.Models.HomeViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace AgriTechUnit.Tests.BusinessManagers
{
    public class HomeBusinessManagerTests
    {
        private readonly Mock<IPostService> _postServiceMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly HomeBusinessManager _manager;

        public HomeBusinessManagerTests()
        {
            _postServiceMock = new Mock<IPostService>();
            _userServiceMock = new Mock<IUserService>();
            _manager = new HomeBusinessManager(_postServiceMock.Object, _userServiceMock.Object);
        }

        [Fact]
        public void GetAuthorViewModel_ReturnsBadRequest_WhenAuthorIdIsNull()
        {
            // Act
            var result = _manager.GetAuthorViewModel(null, "", 1);

            // Assert
            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        public void GetAuthorViewModel_ReturnsNotFound_WhenUserNotFound()
        {
            // Arrange
            _userServiceMock.Setup(u => u.Get("user1")).Returns((ApplicationUser)null);

            // Act
            var result = _manager.GetAuthorViewModel("user1", "", 1);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public void GetAuthorViewModel_ReturnsAuthorViewModel_WhenUserAndPostsExist()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user1", UserName = "TestUser" };
            var posts = new List<Post>
        {
            new Post { Id = 1, Title = "Post 1", Published = true, Creator = user },
            new Post { Id = 2, Title = "Post 2", Published = false, Creator = user }, // Should be excluded
            new Post { Id = 3, Title = "Post 3", Published = true, Creator = user }
        };

            _userServiceMock.Setup(u => u.Get("user1")).Returns(user);
            _postServiceMock.Setup(p => p.GetPosts(It.IsAny<string>())).Returns(posts.AsQueryable());

            // Act
            var result = _manager.GetAuthorViewModel("user1", null, null);

            // Assert
            var okResult = Assert.IsType<AuthorViewModel>(result.Value);
            Assert.Equal(user, okResult.Author);
            Assert.Equal(2, okResult.Posts.Count); // Only 2 posts are published
            Assert.Equal(1, okResult.PageNumber);
        }

        [Fact]
        public void GetAuthorViewModel_UsesSearchStringAndPagingCorrectly()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user2", UserName = "User2" };
            var posts = new List<Post>
        {
            new Post { Id = 1, Title = "Agri", Published = true, Creator = user },
            new Post { Id = 2, Title = "Tech", Published = true, Creator = user },
            new Post { Id = 3, Title = "AgriTech", Published = true, Creator = user }
        };

            _userServiceMock.Setup(u => u.Get("user2")).Returns(user);
            _postServiceMock.Setup(p => p.GetPosts("Agri")).Returns(
                posts.Where(p => p.Title.Contains("Agri")).AsQueryable()
            );

            // Act
            var result = _manager.GetAuthorViewModel("user2", "Agri", 2);

            // Assert
            var vm = Assert.IsType<AuthorViewModel>(result.Value);
            Assert.All(vm.Posts, post => Assert.Contains("Agri", post.Title));
            Assert.Equal("Agri", vm.SearchString);
            Assert.Equal(2, vm.PageNumber);
        }
    }

}
