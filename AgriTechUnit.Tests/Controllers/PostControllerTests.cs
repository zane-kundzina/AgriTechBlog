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
using AgriTechBlog.WebUI.Models.CommentViewModels;

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

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, "user1")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
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

        //[Fact]
        //public async Task Comment_ReturnsJson_WhenAjaxRequestAndCommentCreated()
        //{
        //    // Arrange
        //    var postViewModel = new PostViewModel
        //    {
        //        Post = new Post { Id = 1 },
        //        Comment = new Comment { Content = "Test Comment" }
        //    };

        //    var createdComment = new Comment
        //    {
        //        Content = "Test Comment",
        //        Author = new ApplicationUser { FirstName = "John", LastName = "Doe" },
        //        CreatedOn = DateTime.UtcNow,
        //        Post = postViewModel.Post
        //    };

        //    _postBusinessManagerMock
        //        .Setup(m => m.CreateComment(It.IsAny<PostViewModel>(), It.IsAny<ClaimsPrincipal>()))
        //        .ReturnsAsync(createdComment);

        //    var httpContext = new DefaultHttpContext();
        //    httpContext.Request.Headers["X-Requested-With"] = "XMLHttpRequest";
        //    _controller.ControllerContext = new ControllerContext()
        //    {
        //        HttpContext = httpContext
        //    };

        //    // Act
        //    var result = await _controller.Comment(postViewModel);

        //    // Assert
        //    var jsonResult = Assert.IsType<JsonResult>(result);

        //    // Correct serialization -> deserialization step
        //    var serialized = System.Text.Json.JsonSerializer.Serialize(jsonResult.Value);
        //    var data = System.Text.Json.JsonSerializer.Deserialize<CommentResponse>(serialized);

        //    // Now data is strongly typed!
        //    Assert.True(data.Success);
        //    Assert.Equal("John Doe", data.AuthorName);
        //    Assert.Equal("Test Comment", data.Content);
        //    Assert.False(string.IsNullOrEmpty(data.CreatedOn));
        //}

        //private class CommentResponse
        //{
        //    public bool Success { get; set; }
        //    public string AuthorName { get; set; }
        //    public string Content { get; set; }
        //    public string CreatedOn { get; set; }
        //}

        [Fact]
        public async Task Comment_ReturnsRedirect_WhenNormalRequestAndCommentCreated()
        {
            // Arrange
            var postViewModel = new PostViewModel
            {
                Post = new Post { Id = 1 },
                Comment = new Comment { Content = "Test Comment" }
            };

            var createdComment = new Comment
            {
                Content = "Test Comment",
                Author = new ApplicationUser { FirstName = "John", LastName = "Doe" },
                CreatedOn = DateTime.UtcNow,
                Post = postViewModel.Post
            };

            _postBusinessManagerMock
                .Setup(m => m.CreateComment(It.IsAny<PostViewModel>(), It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(createdComment);

            // No AJAX header set

            // Act
            var result = await _controller.Comment(postViewModel);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal(postViewModel.Post.Id, redirectResult.RouteValues["id"]);
        }

        [Fact]
        public async Task Comment_ReturnsBadRequest_WhenAjaxRequestAndCommentIsNull()
        {
            // Arrange
            var postViewModel = new PostViewModel
            {
                Post = new Post { Id = 1 },
                Comment = new Comment { Content = "Test Comment" }
            };

            _postBusinessManagerMock
                .Setup(m => m.CreateComment(It.IsAny<PostViewModel>(), It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((Comment)null);

            // Properly initialize HttpContext and Request
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["X-Requested-With"] = "XMLHttpRequest";
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            // Act
            var result = await _controller.Comment(postViewModel);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }


        [Fact]
        public async Task Comment_ReturnsRedirect_WhenNormalRequestAndCommentIsNull()
        {
            // Arrange
            var postViewModel = new PostViewModel
            {
                Post = new Post { Id = 1 },
                Comment = new Comment { Content = "Test Comment" }
            };

            _postBusinessManagerMock
                .Setup(m => m.CreateComment(It.IsAny<PostViewModel>(), It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((Comment)null);

            // No AJAX header

            // Act
            var result = await _controller.Comment(postViewModel);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal(postViewModel.Post.Id, redirectResult.RouteValues["Id"]);
        }

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

        [Fact]
        public async Task DeleteComment_ReturnsNotFound_WhenCommentDoesNotExist()
        {
            // Act
            var result = await _controller.DeleteComment(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteComment_ReturnsForbid_WhenUserNotAuthorized()
        {
            // Arrange
            var comment = new Comment { Id = 1, AuthorId = "author1", PostId = 10 };
            _context.Comments.Add(comment);
            _context.Posts.Add(new Post { Id = 10, Creator = new ApplicationUser { Id = "postOwner" } });
            await _context.SaveChangesAsync();

            _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("differentUser");

            // Act
            var result = await _controller.DeleteComment(1);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task DeleteComment_DeletesCommentAndRedirects()
        {
            // Arrange
            var comment = new Comment { Id = 2, AuthorId = "user1", PostId = 20 };
            _context.Comments.Add(comment);
            _context.Posts.Add(new Post { Id = 20, Creator = new ApplicationUser { Id = "user2" } });
            await _context.SaveChangesAsync();

            _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user1");

            // Act
            var result = await _controller.DeleteComment(2);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Post", redirectResult.ControllerName);
            Assert.Equal(20, redirectResult.RouteValues["id"]);

            Assert.Null(await _context.Comments.FindAsync(2));
        }

        [Fact]
        public async Task HideComment_ReturnsNotFound_WhenCommentDoesNotExist()
        {
            // Act
            var result = await _controller.HideComment(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task HideComment_ReturnsForbid_WhenUserNotAuthorized()
        {
            // Arrange
            var comment = new Comment { Id = 3, PostId = 30 };
            _context.Comments.Add(comment);
            _context.Posts.Add(new Post { Id = 30, Creator = new ApplicationUser { Id = "ownerId1" } });
            await _context.SaveChangesAsync();

            _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("differentUser");

            // Act
            var result = await _controller.HideComment(3);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task HideComment_TogglesIsHidden_AndRedirects()
        {
            // Arrange
            var comment = new Comment { Id = 4, PostId = 40 };
            _context.Comments.Add(comment);
            _context.Posts.Add(new Post { Id = 40, Creator = new ApplicationUser { Id = "ownerId" } });
            await _context.SaveChangesAsync();

            _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("ownerId");

            // Act
            var result = await _controller.HideComment(4);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.True(_context.Comments.Find(4).IsHidden);
        }

        [Fact]
        public async Task ToggleHideComment_ReturnsNotFound_WhenCommentDoesNotExist()
        {
            // Act
            var result = await _controller.ToggleHideComment(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        //[Fact]
        //public async Task ToggleHideComment_TogglesAndReturnsJson()
        //{
        //    // Arrange
        //    var comment = new Comment { Id = 5, PostId = 50 };
        //    _context.Comments.Add(comment);
        //    await _context.SaveChangesAsync();

        //    // Act
        //    var result = await _controller.ToggleHideComment(5);

        //    // Assert
        //    var jsonResult = Assert.IsType<JsonResult>(result);
        //    var data = (dynamic)jsonResult.Value;
        //    Assert.True((bool)data.success);
        //    Assert.True((bool)data.isHidden);
        //}

        [Fact]
        public async Task EditComment_Get_ReturnsNotFound_WhenCommentDoesNotExist()
        {
            // Act
            var result = await _controller.EditComment(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditComment_Get_ReturnsForbid_WhenUserNotAuthor()
        {
            // Arrange
            var comment = new Comment { Id = 6, AuthorId = "authorId" };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("differentUser");

            // Act
            var result = await _controller.EditComment(6);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task EditComment_Get_ReturnsView_WithCorrectModel()
        {
            // Arrange
            var comment = new Comment { Id = 7, AuthorId = "authorId", Content = "Original Content" };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("authorId");

            // Act
            var result = await _controller.EditComment(7);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<EditCommentViewModel>(viewResult.Model);
            Assert.Equal(7, model.CommentId);
            Assert.Equal("Original Content", model.Content);
        }

        [Fact]
        public async Task EditComment_Post_ReturnsNotFound_WhenCommentDoesNotExist()
        {
            // Arrange
            var model = new EditCommentViewModel { CommentId = 999, Content = "Updated Content" };

            // Act
            var result = await _controller.EditComment(model);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditComment_Post_ReturnsForbid_WhenUserNotAuthor()
        {
            // Arrange
            var comment = new Comment { Id = 8, AuthorId = "authorId", Content = "Old Content" };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("differentUser");

            var model = new EditCommentViewModel { CommentId = 8, Content = "Updated Content" };

            // Act
            var result = await _controller.EditComment(model);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task EditComment_Post_UpdatesContent_AndRedirects()
        {
            // Arrange
            var comment = new Comment { Id = 9, AuthorId = "authorId", PostId = 90, Content = "Old Content" };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("authorId");

            var model = new EditCommentViewModel { CommentId = 9, Content = "Updated Content" };

            // Act
            var result = await _controller.EditComment(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Post", redirectResult.ControllerName);

            var updatedComment = await _context.Comments.FindAsync(9);
            Assert.Equal("Updated Content", updatedComment.Content);
        }

    }
}
