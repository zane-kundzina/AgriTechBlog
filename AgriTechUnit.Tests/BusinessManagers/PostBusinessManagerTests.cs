using AgriTechBlog.Data.Models;
using AgriTechBlog.Service.Interfaces;
using AgriTechBlog.WebUI.Authorization;
using AgriTechBlog.WebUI.BusinessManagers;
using AgriTechBlog.WebUI.Models.PostViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AgriTechUnit.Tests.BusinessManagers
{
    public class PostBusinessManagerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<IPostService> _postServiceMock;
        private readonly Mock<IWebHostEnvironment> _envMock;
        private readonly Mock<IAuthorizationService> _authServiceMock;

        private readonly PostBusinessManager _manager;

        public PostBusinessManagerTests()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
            _postServiceMock = new Mock<IPostService>();
            _envMock = new Mock<IWebHostEnvironment>();
            _authServiceMock = new Mock<IAuthorizationService>();

            _manager = new PostBusinessManager(
                _userManagerMock.Object,
                _postServiceMock.Object,
                _envMock.Object,
                _authServiceMock.Object
            );
        }

        [Fact]
        public void GetIndexViewModel_Returns_CorrectData()
        {
            // Arrange
            var samplePosts = new List<Post>
            {
                new Post { Title = "Post A", Published = true, UpdatedDate = DateTime.Now },
                new Post { Title = "Post B", Published = true, UpdatedDate = DateTime.Now.AddDays(-1) }
            };

            _postServiceMock.Setup(p => p.GetPosts(It.IsAny<string>()))
                            .Returns(samplePosts.AsQueryable());

            // Act
            var result = _manager.GetIndexViewModel(null, null);

            // Assert
            Assert.Equal(2, result.Posts.Count());
            Assert.Contains(result.Posts, p => p.Title == "Post A");
        }

        [Fact]
        public void GetIndexViewModel_ReturnsCorrectModel()
        {
            // Arrange
            var posts = new List<Post>
            {
                new Post
                {
                    Id = 1,
                    Title = "Test 1",
                    Published = true,
                    UpdatedDate = DateTime.Now,
                    Reactions = new List<PostReaction> { new PostReaction { IsLike = true } },
                    Comments = new List<Comment> { new Comment { CreatedOn = DateTime.Now } }
                }
            }.AsQueryable();

            _postServiceMock.Setup(x => x.GetPosts(It.IsAny<string>())).Returns(posts);

            // Act
            var result = _manager.GetIndexViewModel("", 1);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Posts);
            Assert.Single(result.LastCommentedPosts);
            Assert.Single(result.MostLikedPosts);
        }

        [Fact]
        public async Task GetPostViewModel_ReturnsNotFound_WhenPostIsNull()
        {
            // Arrange
            int? postId = 1;
            _postServiceMock.Setup(p => p.GetPost(postId.Value)).Returns((Post)null);

            // Act
            var result = await _manager.GetPostViewModel(postId, new ClaimsPrincipal());

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetPostViewModel_ReturnsBadRequest_WhenIdIsNull()
        {
            var result = await _manager.GetPostViewModel(null, new ClaimsPrincipal());
            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        public async Task GetPostViewModel_ReturnsChallenge_WhenUnauthorizedAndNotAuthenticated()
        {
            // Arrange
            var post = new Post { Id = 1, Published = false };

            _postServiceMock.Setup(p => p.GetPost(1)).Returns(post);

            var identity = new ClaimsIdentity(); // No authentication type → Not authenticated
            var claims = new ClaimsPrincipal(identity);

            _authServiceMock
            .Setup(a => a.AuthorizeAsync(
                It.Is<ClaimsPrincipal>(c => c == claims),
                It.Is<object>(r => r == post),
                It.Is<IAuthorizationRequirement[]>(reqs => reqs.Length == 1 && reqs[0] == Operations.Read)))
            .ReturnsAsync(AuthorizationResult.Failed());

            // Act
            var result = await _manager.GetPostViewModel(1, claims);

            // Assert
            Assert.IsType<ChallengeResult>(result.Result);
        }

        [Fact]
        public async Task GetEditViewModel_ReturnsForbid_WhenUnauthorized()
        {
            // Arrange
            var post = new Post { Id = 1 };
            _postServiceMock.Setup(p => p.GetPost(1)).Returns(post);

            var identity = new ClaimsIdentity("TestAuthType");
            identity.AddClaim(new Claim(ClaimTypes.Name, "TestUser"));
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _authServiceMock.Setup(a =>
                a.AuthorizeAsync(claimsPrincipal, post, It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(AuthorizationResult.Failed());

            // Act
            var result = await _manager.GetEditViewModel(1, claimsPrincipal);

            // Assert
            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task CreatePost_ShouldCreatePostAndSaveImage()
        {
            // Arrange
            var mockUserManager = MockUserManager<ApplicationUser>();
            var mockPostService = new Mock<IPostService>();
            var mockEnv = new Mock<IWebHostEnvironment>();
            var mockAuth = new Mock<IAuthorizationService>();

            var user = new ApplicationUser { UserName = "testuser" };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "testuser")
            }));

            mockUserManager.Setup(m => m.GetUserAsync(claimsPrincipal)).ReturnsAsync(user);

            var post = new Post { Id = 1, Title = "Test Post", Content = "Test Content" };
            mockPostService.Setup(m => m.Add(It.IsAny<Post>())).ReturnsAsync(post);

            var tempFolder = Path.GetTempPath();
            mockEnv.Setup(e => e.WebRootPath).Returns(tempFolder);

            var fileMock = new Mock<IFormFile>();
            var content = "Fake image content";
            var fileName = "HeaderImage.jpg";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;
            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(ms.Length);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default)).Returns<Stream, CancellationToken>((stream, token) =>
            {
                return ms.CopyToAsync(stream, token);
            });

            var createViewModel = new CreateViewModel
            {
                Post = post,
                HeaderImage = fileMock.Object
            };

            var manager = new PostBusinessManager(mockUserManager.Object, mockPostService.Object, mockEnv.Object, mockAuth.Object);

            // Act
            var result = await manager.CreatePost(createViewModel, claimsPrincipal);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Post", result.Title);
            Assert.True(File.Exists(Path.Combine(tempFolder, "UserFiles", "Posts", "1", "HeaderImage.jpg")));
        }

        [Fact]
        public async Task UpdatePost_ShouldUpdatePostAndReplaceImage()
        {
            // Arrange
            var mockUserManager = MockUserManager<ApplicationUser>();
            var mockPostService = new Mock<IPostService>();
            var mockEnv = new Mock<IWebHostEnvironment>();
            var mockAuth = new Mock<IAuthorizationService>();

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "testuser")
            }));

            var existingPost = new Post
            {
                Id = 2,
                Title = "Old Title",
                Content = "Old Content",
                Published = false
            };

            mockPostService.Setup(s => s.GetPost(2)).Returns(existingPost);
            mockPostService.Setup(s => s.Update(It.IsAny<Post>())).ReturnsAsync((Post p) => p);

            mockAuth.Setup(a => a.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.Is<IEnumerable<IAuthorizationRequirement>>(r => r
                    .OfType<OperationAuthorizationRequirement>()
                    .Any(req => req.Name == Operations.Update.Name)))
            )
            .ReturnsAsync(AuthorizationResult.Success());

            var tempFolder = Path.GetTempPath();
            mockEnv.Setup(e => e.WebRootPath).Returns(tempFolder);

            var fileMock = new Mock<IFormFile>();
            var content = "New image";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default)).Returns<Stream, CancellationToken>((stream, token) =>
            {
                return ms.CopyToAsync(stream, token);
            });

            var editViewModel = new EditViewModel
            {
                Post = new Post
                {
                    Id = 2,
                    Title = "Updated Title",
                    Content = "Updated Content",
                    Published = true
                },
                HeaderImage = fileMock.Object
            };

            var manager = new PostBusinessManager(mockUserManager.Object, mockPostService.Object, mockEnv.Object, mockAuth.Object);

            // Act
            var result = await manager.UpdatePost(editViewModel, claimsPrincipal);

            // Assert
            Assert.IsType<EditViewModel>(result.Value);
            Assert.Equal("Updated Title", result.Value.Post.Title);
            Assert.True(File.Exists(Path.Combine(tempFolder, "UserFiles", "Posts", "2", "HeaderImage.jpg")));
        }

        [Fact]
        public async Task UpdatePost_ReturnsNotFound_WhenPostIsNull()
        {
            _postServiceMock.Setup(p => p.GetPost(It.IsAny<int>())).Returns((Post)null);
            var result = await _manager.UpdatePost(new EditViewModel { Post = new Post { Id = 1 } }, new ClaimsPrincipal());
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateComment_ReturnsBadRequest_WhenPostIsNull()
        {
            // Arrange
            var viewModel = new PostViewModel
            {
                Post = null
            };

            // Act
            var result = await _manager.CreateComment(viewModel, new ClaimsPrincipal());

            // Assert
            Assert.IsType<Microsoft.AspNetCore.Mvc.BadRequestResult>(result.Result);
        }

        [Fact]
        public async Task CreateComment_ReturnsBadRequest_WhenPostIdIsInvalid()
        {
            var vm = new PostViewModel { Post = new Post { Id = 0 } };
            var result = await _manager.CreateComment(vm, new ClaimsPrincipal());
            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        public async Task CreateComment_ReturnsNotFound_WhenPostIsMissing()
        {
            _postServiceMock.Setup(p => p.GetPost(It.IsAny<int>())).Returns((Post)null);
            var vm = new PostViewModel { Post = new Post { Id = 1 } };
            var result = await _manager.CreateComment(vm, new ClaimsPrincipal());
            Assert.IsType<NotFoundResult>(result.Result);
        }

        private static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
        {
            var store = new Mock<IUserStore<TUser>>();
            return new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
        }
    }
}
