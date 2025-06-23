using AuctionApp.Controllers;
using AuctionApp.Data;
using AuctionApp.Models;
using AuctionApp.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Xunit;

namespace AuctionApp.Tests.Controllers
{
    public class UserControllerTests
    {
        private (UserController controller, AppDbContext context) CreateController()
        {
            var context = TestDbContextFactory.Create();
            return (new UserController(context), context);
        }
        [Fact]
        public async Task GetUsers_ReturnsAllUsers()
        {
            var (controller, context) = CreateController();

            context.Users.Add(new ApplicationUser { Id = "user1", UserName = "user1@example.com" });
            context.Users.Add(new ApplicationUser { Id = "user2", UserName = "user2@example.com" });
            await context.SaveChangesAsync();

            var actionResult = await controller.GetUsers();

            var users = Assert.IsAssignableFrom<List<ApplicationUser>>(actionResult.Value);
            Assert.Equal(2, users.Count);
        }

        [Fact]
        public async Task GetApplicationUser_ExistingUser_ReturnsOk()
        {
            var (controller, context) = CreateController();

            var user = new ApplicationUser { Id = "user1", UserName = "test@user.com" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var actionResult = await controller.GetApplicationUser("user1");

            Assert.NotNull(actionResult); 

            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.NotNull(okResult);

            var returnedUser = Assert.IsType<ApplicationUser>(okResult.Value);
            Assert.Equal("user1", returnedUser.Id);
        }

        [Fact]
        public async Task GetApplicationUser_NonExistingUser_ReturnsNotFound()
        {
            var (controller, _) = CreateController();

            var actionResult = await controller.GetApplicationUser("nonexistent");

            Assert.IsType<NotFoundResult>(actionResult.Result);
        }

        [Fact]
        public async Task PostApplicationUser_ValidUser_ReturnsCreated()
        {
            var (controller, context) = CreateController();

            var user = new ApplicationUser { Id = "user1", UserName = "newuser@example.com" };

            var actionResult = await controller.PostApplicationUser(user);

            var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var createdUser = Assert.IsType<ApplicationUser>(createdResult.Value);
            Assert.Equal(user.Id, createdUser.Id);
        }

        [Fact]
        public async Task PostApplicationUser_Conflict_ReturnsConflict()
        {
            var (controller, context) = CreateController();

            var user = new ApplicationUser { Id = "user1", UserName = "user@example.com" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

           
            context.Entry(user).State = Microsoft.EntityFrameworkCore.EntityState.Detached;

          
            var duplicateUser = new ApplicationUser { Id = "user1", UserName = "duplicate@example.com" };

            var actionResult = await controller.PostApplicationUser(duplicateUser);

            Assert.IsType<ConflictResult>(actionResult.Result);
        }

        [Fact]
        public async Task PutApplicationUser_ValidUpdate_ReturnsNoContent()
        {
            var (controller, context) = CreateController();

            var user = new ApplicationUser { Id = "user1", UserName = "user@example.com" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            user.UserName = "updated@example.com";

            var result = await controller.PutApplicationUser(user.Id, user);

            Assert.IsType<NoContentResult>(result);

            var updatedUser = await context.Users.FindAsync(user.Id);
            Assert.Equal("updated@example.com", updatedUser.UserName);
        }

        [Fact]
        public async Task PutApplicationUser_IdMismatch_ReturnsBadRequest()
        {
            var (controller, _) = CreateController();

            var user = new ApplicationUser { Id = "user1", UserName = "user@example.com" };

            var result = await controller.PutApplicationUser("differentId", user);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task PutApplicationUser_NonExistingUser_ReturnsNotFound()
        {
            var (controller, _) = CreateController();

            var user = new ApplicationUser { Id = "nonexistent", UserName = "user@example.com" };

            var result = await controller.PutApplicationUser(user.Id, user);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteApplicationUser_ExistingUser_ReturnsNoContent()
        {
            var (controller, context) = CreateController();

            var user = new ApplicationUser { Id = "user1", UserName = "user@example.com" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var result = await controller.DeleteApplicationUser(user.Id);

            Assert.IsType<NoContentResult>(result);

            var deletedUser = await context.Users.FindAsync(user.Id);
            Assert.Null(deletedUser);
        }

        [Fact]
        public async Task DeleteApplicationUser_NonExistingUser_ReturnsNotFound()
        {
            var (controller, _) = CreateController();

            var result = await controller.DeleteApplicationUser("nonexistent");

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
