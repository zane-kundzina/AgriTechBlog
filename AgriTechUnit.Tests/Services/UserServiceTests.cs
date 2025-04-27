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
    public class UserServiceTests
    {
        private DbContextOptions<ApplicationDbContext> CreateNewContextOptions()
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public void Get_ShouldReturnUserById()
        {
            // Arrange
            var options = CreateNewContextOptions();
            using var context = new ApplicationDbContext(options);
            var user = new ApplicationUser { Id = "user1", UserName = "TestUser" };
            context.Users.Add(user);
            context.SaveChanges();

            var service = new UserService(context);

            // Act
            var result = service.Get("user1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TestUser", result.UserName);
        }

        [Fact]
        public void GetAll_ShouldReturnAllUsers()
        {
            // Arrange
            var options = CreateNewContextOptions();
            using var context = new ApplicationDbContext(options);
            context.Users.AddRange(
                new ApplicationUser { Id = "user1", UserName = "User1" },
                new ApplicationUser { Id = "user2", UserName = "User2" }
            );
            context.SaveChanges();

            var service = new UserService(context);

            // Act
            var result = service.GetAll();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task Update_ShouldSaveUserChanges()
        {
            // Arrange
            var options = CreateNewContextOptions();
            using var context = new ApplicationDbContext(options);
            var user = new ApplicationUser { Id = "user1", UserName = "OldName" };
            context.Users.Add(user);
            context.SaveChanges();

            var service = new UserService(context);

            // Act
            user.UserName = "UpdatedName";
            await service.Update(user);

            // Assert
            var updatedUser = context.Users.FirstOrDefault(u => u.Id == "user1");
            Assert.NotNull(updatedUser);
            Assert.Equal("UpdatedName", updatedUser.UserName);
        }
    }
}
