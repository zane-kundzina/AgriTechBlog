using AgriTechBlog.Data.Models;
using AgriTechBlog.Service.Interfaces;
using AgriTechBlog.WebUI.BusinessManagers;
using AgriTechBlog.WebUI.Models.AdminViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Xunit;
using System.Linq;

namespace AgriTechUnit.Tests.BusinessManagers
{
    public class AdminBusinessManagerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<IPostService> _postServiceMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IWebHostEnvironment> _envMock;

        private readonly AdminBusinessManager _manager;

        public AdminBusinessManagerTests()
        {
            _userManagerMock = MockUserManager<ApplicationUser>();
            _postServiceMock = new Mock<IPostService>();
            _userServiceMock = new Mock<IUserService>();
            _envMock = new Mock<IWebHostEnvironment>();

            _manager = new AdminBusinessManager(
                _userManagerMock.Object,
                _postServiceMock.Object,
                _userServiceMock.Object,
                _envMock.Object
            );
        }

        [Fact]
        public async Task GetAdminDashboard_ReturnsPostsForUser()
        {
            var user = new ApplicationUser { Id = "123" };
            var claims = new ClaimsPrincipal();
            var posts = new List<Post> { new Post { Id = 1 } }.AsQueryable();

            _userManagerMock.Setup(x => x.GetUserAsync(claims)).ReturnsAsync(user);
            _postServiceMock.Setup(p => p.GetPosts(user)).Returns(posts);

            var result = await _manager.GetAdminDashboard(claims);

            Assert.NotNull(result);
            Assert.Equal(posts, result.Posts);
        }

        [Fact]
        public async Task GetAboutViewModel_ReturnsCorrectModel()
        {
            var user = new ApplicationUser { Id = "123", SubHeader = "Hello", AboutContent = "Welcome" };
            var claims = new ClaimsPrincipal();

            _userManagerMock.Setup(x => x.GetUserAsync(claims)).ReturnsAsync(user);

            var result = await _manager.GetAboutViewModel(claims);

            Assert.NotNull(result);
            Assert.Equal(user, result.ApplicationUser);
            Assert.Equal("Hello", result.SubHeader);
            Assert.Equal("Welcome", result.Content);
        }

        [Fact]
        public async Task UpdateAbout_UpdatesUserContent_AndSavesHeaderImage()
        {
            var user = new ApplicationUser { Id = "user1" };
            var claims = new ClaimsPrincipal();
            var envPath = Path.GetTempPath();
            var memoryStream = new MemoryStream();
            var fileMock = new Mock<IFormFile>();

            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default)).Returns<Stream, CancellationToken>((stream, token) =>
            {
                return memoryStream.CopyToAsync(stream, token);
            });

            var model = new AboutViewModel
            {
                SubHeader = "Updated subheader",
                Content = "Updated content",
                HeaderImage = fileMock.Object
            };

            _userManagerMock.Setup(x => x.GetUserAsync(claims)).ReturnsAsync(user);
            _userServiceMock.Setup(x => x.Update(user)).ReturnsAsync(user);
            _envMock.Setup(e => e.WebRootPath).Returns(envPath);

            await _manager.UpdateAbout(model, claims);

            Assert.Equal("Updated subheader", user.SubHeader);
            Assert.Equal("Updated content", user.AboutContent);

            string expectedPath = Path.Combine(envPath, "UserFiles", "Users", "user1", "HeaderImage.jpg");
            Assert.True(File.Exists(expectedPath));

            File.Delete(expectedPath); // cleanup
        }

        private static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
        {
            var store = new Mock<IUserStore<TUser>>();
            return new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
        }
    }
}
