using AgriTechBlog.Data.Models;
using AgriTechBlog.Data;
using AgriTechBlog.WebUI.BusinessManagers.Interfaces;
using AgriTechBlog.WebUI.Controllers;
using AgriTechBlog.WebUI.Models.PostViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace AgriTechUnit.Tests.Controllers
{
    public class PostControllerTests
    {
        private readonly Mock<IPostBusinessManager> _postBusinessManagerMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly ApplicationDbContext _context;
        private readonly PostController _controller;

        public PostControllerTests()
        {
            _postBusinessManagerMock = new Mock<IPostBusinessManager>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "PostControllerTests")
                .Options;
            _context = new ApplicationDbContext(options);

            _controller = new PostController(_context, _postBusinessManagerMock.Object, _userManagerMock.Object);
        }

        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // unique DB per test
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Index_Returns_View_When_Result_Is_Null()
        {
            // Arrange
            var viewModel = new PostViewModel();
            _postBusinessManagerMock.Setup(x => x.GetPostViewModel(It.IsAny<int?>(), It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new ActionResult<PostViewModel>(viewModel));

            // Act
            var result = await _controller.Index(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(viewModel, viewResult.Model);
        }

        [Fact]
        public void Create_Returns_View_With_CreateViewModel()
        {
            // Act
            var result = _controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<CreateViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task Edit_Returns_View_When_Result_Is_Null()
        {
            // Arrange
            var viewModel = new EditViewModel();
            _postBusinessManagerMock.Setup(x => x.GetEditViewModel(It.IsAny<int?>(), It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new ActionResult<EditViewModel>(viewModel));

            // Act
            var result = await _controller.Edit(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(viewModel, viewResult.Model);
        }

        [Fact]
        public async Task Add_Redirects_To_Admin_Index()
        {
            // Act
            var result = await _controller.Add(new CreateViewModel());

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Admin", redirect.ControllerName);
        }

        [Fact]
        public async Task Update_Redirects_To_Edit_When_Validation_Fails()
        {
            // Arrange
            var editViewModel = new EditViewModel { Post = new Post { Id = 1 } };
            _postBusinessManagerMock.Setup(x => x.UpdatePost(It.IsAny<EditViewModel>(), It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new ActionResult<EditViewModel>(new RedirectToActionResult("Edit", "Post", new { id = 1 })));

            // Act
            var result = await _controller.Update(editViewModel);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
        }

        //[Fact]
        //public async Task Comment_Redirects_To_Post_Index()
        //{
        //    // Arrange
        //    var postViewModel = new PostViewModel { Post = new Post { Id = 1 } };
        //    _postBusinessManagerMock.Setup(x => x.CreateComment(It.IsAny<PostViewModel>(), It.IsAny<ClaimsPrincipal>()))
        //        .ReturnsAsync(new ActionResult<Comment>(new Comment()));

        //    // Act
        //    var result = await _controller.Comment(postViewModel);

        //    // Assert
        //    Assert.IsType<RedirectToActionResult>(result);
        //}

        [Fact]
        public void ReactToPost_Adds_New_Reaction()
        {
            // Arrange
            using var context = GetInMemoryDbContext(); // new fresh context
            var userId = "testUser";

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            }));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            _userManagerMock.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);

            // Act
            var result = _controller.ReactToPost(1, true);

            // Assert
            var redirect = Assert.IsType<RedirectToRouteResult>(result);
            Assert.Equal("PostDetails", redirect.RouteName);
        }

        [Fact]
        public async Task Delete_Returns_NotFound_When_Post_Not_Found()
        {
            // Act
            var result = await _controller.Delete(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ToggleVisibility_Returns_NotFound_When_Post_Not_Found()
        {
            // Act
            var result = await _controller.ToggleVisibility(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void ReactToPost_WhenUserUnreacts_ShouldRemoveReaction()
        {
            // Arrange            
            using var context = GetInMemoryDbContext(); // new fresh context
            var postId = 1;
            var userId = "testUser";
            var isLike = true;
            var existingReaction = new PostReaction { PostId = postId, UserId = userId, IsLike = isLike };

            context.PostReactions.Add(existingReaction);
            context.SaveChanges();

            _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);

            var controller = new PostController(context, _postBusinessManagerMock.Object, _userManagerMock.Object);

            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            // Act
            var result = controller.ReactToPost(postId, isLike);

            foreach (var entry in context.ChangeTracker.Entries())
            {
                entry.State = EntityState.Detached;
            }

            // Assert
            Assert.Null(context.PostReactions.FirstOrDefault(r => r.PostId == postId && r.UserId == userId));
        }


        [Fact]
        public void ReactToPost_WhenUserChangesReaction_ShouldUpdateReaction()
        {
            // Arrange
            using var context = GetInMemoryDbContext(); // new fresh context
            var postId = 1;
            var userId = "testUser";
            var existingReaction = new PostReaction { PostId = postId, UserId = userId, IsLike = false };

            context.PostReactions.Add(existingReaction);
            context.SaveChanges();

            _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);

            var controller = new PostController(context, _postBusinessManagerMock.Object, _userManagerMock.Object);

            // Act
            var result = controller.ReactToPost(postId, true); // switch to Like

            // Assert
            var updatedReaction = context.PostReactions.FirstOrDefault(r => r.PostId == postId && r.UserId == userId);
            Assert.NotNull(updatedReaction);
            Assert.True(updatedReaction.IsLike); // now it should be true
        }

        [Fact]
        public async Task Delete_WhenUserIsNotCreatorAndNotAdmin_ShouldReturnForbid()
        {
            // Arrange
            using var context = GetInMemoryDbContext(); // new fresh context
            var postId = 1;
            var post = new Post { Id = postId, CreatorID = "otherUser" };
            context.Posts.Add(post);
            await context.SaveChangesAsync();

            _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("currentUser");
            var controller = new PostController(context, _postBusinessManagerMock.Object, _userManagerMock.Object);

            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.IsInRole("Admin")).Returns(false);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userMock.Object }
            };

            // Act
            var result = await controller.Delete(postId);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Delete_WhenUserIsCreator_ShouldDeletePostAndRedirect()
        {
            // Arrange
            using var context = GetInMemoryDbContext(); // new fresh context
            var postId = 1;
            var userId = "creatorUserId";
            var post = new Post { Id = postId, CreatorID = userId };
            context.Posts.Add(post);
            await context.SaveChangesAsync();

            _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);

            var controller = new PostController(context, _postBusinessManagerMock.Object, _userManagerMock.Object);

            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.IsInRole("Admin")).Returns(false);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userMock.Object }
            };

            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            // Act
            var result = await controller.Delete(postId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Admin", redirectResult.ControllerName);
            Assert.Null(context.Posts.FirstOrDefault(p => p.Id == postId)); // Post should be deleted
        }

        [Fact]
        public async Task ToggleVisibility_WhenUserIsNotCreatorAndNotAdmin_ShouldReturnForbid()
        {
            // Arrange
            using var context = GetInMemoryDbContext(); // new fresh context
            var postId = 1;
            var post = new Post { Id = postId, Creator = new ApplicationUser { Id = "creatorUser" }, Published = true };
            context.Posts.Add(post);
            await context.SaveChangesAsync();

            _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("currentUser");

            var controller = new PostController(context, _postBusinessManagerMock.Object, _userManagerMock.Object);
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.IsInRole("Admin")).Returns(false);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userMock.Object }
            };

            // Act
            var result = await controller.ToggleVisibility(postId);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task ToggleVisibility_WhenUserIsCreator_ShouldTogglePublishStatusAndRedirect()
        {
            // Arrange
            using var context = GetInMemoryDbContext(); // new fresh context
            var postId = 1;
            var userId = "currentUser";

            var creatorUser = new ApplicationUser { Id = userId };
            context.Users.Add(creatorUser); // Attach Creator separately
            await context.SaveChangesAsync();

            var post = new Post { Id = postId, Creator = creatorUser, Published = false };
            context.Posts.Add(post);
            await context.SaveChangesAsync();

            _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);

            var controller = new PostController(context, _postBusinessManagerMock.Object, _userManagerMock.Object);
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.IsInRole("Admin")).Returns(false);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userMock.Object }
            };

            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            // Act
            var result = await controller.ToggleVisibility(postId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Admin", redirectResult.ControllerName);

            var updatedPost = context.Posts.FirstOrDefault(p => p.Id == postId);
            Assert.True(updatedPost.Published); // should be toggled to true
        }

        [Fact]
        public async Task ToggleVisibility_WhenUserIsAdmin_ShouldTogglePublishStatusAndRedirect()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var postId = 1;
            var creatorUser = new ApplicationUser { Id = "anotherUser" };

            context.Users.Add(creatorUser); // Attach Creator separately
            await context.SaveChangesAsync();

            var post = new Post { Id = postId, Creator = creatorUser, Published = true };
            context.Posts.Add(post);
            await context.SaveChangesAsync();

            _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("currentUser");

            var controller = new PostController(context, _postBusinessManagerMock.Object, _userManagerMock.Object);
            var userMock = new Mock<ClaimsPrincipal>();
            userMock.Setup(u => u.IsInRole("Admin")).Returns(true);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userMock.Object }
            };

            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            // Act
            var result = await controller.ToggleVisibility(postId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Admin", redirectResult.ControllerName);

            var updatedPost = context.Posts.FirstOrDefault(p => p.Id == postId);
            Assert.False(updatedPost.Published); // should be toggled to false
        }


    }
}
