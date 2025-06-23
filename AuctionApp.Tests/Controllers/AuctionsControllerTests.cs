using AuctionApp.Controllers;
using AuctionApp.Data;
using AuctionApp.Models;
using AuctionApp.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace AuctionApp.Tests.Controllers
{
    public class AuctionsControllerTests
    {
        private (AuctionsController controller, AppDbContext context) CreateControllerWithUser(string userId)
        {
            var context = TestDbContextFactory.Create();
            var controller = new AuctionsController(context);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            return (controller, context);
        }

        [Fact]
        public async Task CreateAuction_ReturnsCreated()
        {
            var (controller, _) = CreateControllerWithUser("user1");
            var auction = new Auction
            {
                StartPrice = 100,
                Title = "Test Auction",
                Description = "Test Description"
            };

            var result = await controller.CreateAuction(auction);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var createdAuction = Assert.IsType<Auction>(createdResult.Value);
            Assert.Equal("user1", createdAuction.OwnerId);
            Assert.Equal(AuctionStatus.Active, createdAuction.Status);
            Assert.Equal(100, createdAuction.CurrentPrice);
        }

        [Fact]
        public async Task GetAuction_ReturnsAllAuctions()
        {
            var (controller, context) = CreateControllerWithUser("user1");

            context.Auctions.Add(new Auction { Title = "A1", Description = "test", StartPrice = 50, CurrentPrice = 50, Status = AuctionStatus.Active, OwnerId = "user1" });
            context.Auctions.Add(new Auction { Title = "A2", Description = "test", StartPrice = 100, CurrentPrice = 100, Status = AuctionStatus.Active, OwnerId = "user2" });
            await context.SaveChangesAsync();

            var result = await controller.GetAuction();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var auctions = Assert.IsAssignableFrom<System.Collections.Generic.List<Auction>>(okResult.Value);
            Assert.Equal(2, auctions.Count);
        }

        [Fact]
        public async Task GetAuctionById_Existing_ReturnsAuction()
        {
            var (controller, context) = CreateControllerWithUser("user1");

            var auction = new Auction { Title = "A1", Description = "test", StartPrice = 50, CurrentPrice = 50, Status = AuctionStatus.Active, OwnerId = "user1" };
            context.Auctions.Add(auction);
            await context.SaveChangesAsync();

            var result = await controller.GetAuction(auction.Id);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedAuction = Assert.IsType<Auction>(okResult.Value);
            Assert.Equal(auction.Id, returnedAuction.Id);
        }
        
        [Fact]
        public async Task GetAuctionById_NotFound_ReturnsNotFound()
        {
            var (controller, _) = CreateControllerWithUser("user1");

            var result = await controller.GetAuction(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteAuction_Existing_ReturnsNoContent()
        {
            var (controller, context) = CreateControllerWithUser("user1");

            var auction = new Auction { Title = "A1", Description = "test", StartPrice = 50, CurrentPrice = 50, Status = AuctionStatus.Active, OwnerId = "user1" };
            context.Auctions.Add(auction);
            await context.SaveChangesAsync();

            var result = await controller.DeleteAuction(auction.Id);

            Assert.IsType<NoContentResult>(result);
            Assert.Null(await context.Auctions.FindAsync(auction.Id));
        }

        [Fact]
        public async Task DeleteAuction_NotFound_ReturnsNotFound()
        {
            var (controller, _) = CreateControllerWithUser("user1");

            var result = await controller.DeleteAuction(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CloseAuction_Valid_ChangesStatus()
        {
            var (controller, context) = CreateControllerWithUser("user1");

            var auction = new Auction { Title = "A1", Description = "test", StartPrice = 50, CurrentPrice = 75, Status = AuctionStatus.Active, OwnerId = "user1" };
            context.Auctions.Add(auction);
            await context.SaveChangesAsync();

            var result = await controller.CloseAuction(auction.Id);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedAuction = Assert.IsType<Auction>(okResult.Value);

            Assert.Equal(AuctionStatus.Closing, updatedAuction.Status);
        }

        [Fact]
        public async Task CloseAuction_AlreadyClosed_ReturnsBadRequest()
        {
            var (controller, context) = CreateControllerWithUser("user1");

            var auction = new Auction { Title = "A1", Description = "test", StartPrice = 50, CurrentPrice = 75, Status = AuctionStatus.Sold, OwnerId = "user1" };
            context.Auctions.Add(auction);
            await context.SaveChangesAsync();

            var result = await controller.CloseAuction(auction.Id);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("уже закрыт", badRequest.Value.ToString());
        }

        [Fact]
        public async Task CloseAuction_NotFound_ReturnsNotFound()
        {
            var (controller, _) = CreateControllerWithUser("user1");

            var result = await controller.CloseAuction(999);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("не найден", notFound.Value.ToString());
        }

        [Fact]
        public async Task MarkSold_Valid_ChangesStatus()
        {
            var (controller, context) = CreateControllerWithUser("user1");

            var auction = new Auction { Title = "A1", Description = "test", StartPrice = 50, CurrentPrice = 75, Status = AuctionStatus.Closing, OwnerId = "user1" };
            context.Auctions.Add(auction);
            await context.SaveChangesAsync();

            var result = await controller.MarkSold(auction.Id);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedAuction = Assert.IsType<Auction>(okResult.Value);

            Assert.Equal(AuctionStatus.Sold, updatedAuction.Status);
        }

        [Fact]
        public async Task MarkSold_NotOwner_ReturnsNotFound()
        {
            var (controller, context) = CreateControllerWithUser("user1");

            var auction = new Auction { Title = "A1", Description = "test", StartPrice = 50, CurrentPrice = 75, Status = AuctionStatus.Closing, OwnerId = "user2" };
            context.Auctions.Add(auction);
            await context.SaveChangesAsync();

            var result = await controller.MarkSold(auction.Id);

            Assert.IsType<NotFoundResult>(result);
        }


    }
}
