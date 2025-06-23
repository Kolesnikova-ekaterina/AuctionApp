using AuctionApp.Controllers;
using AuctionApp.Data;
using AuctionApp.Models;
using AuctionApp.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Xunit;

namespace AuctionApp.Tests.Controllers
{
    public class BidControllerTests
    {
        private (BidController controller, AppDbContext context) CreateController()
        {
            var context = TestDbContextFactory.Create();
            return (new BidController(context), context);
        }
        [Fact]
        public async Task GetBids_ReturnsAllBids()
        {
            var (controller, context) = CreateController();
            var user = new ApplicationUser { Id = "user1", UserName = "testuser@example.com" };
             context.Users.Add(user);

            
            var auction = new Auction
            {
                OwnerId = user.Id,
                StartPrice = 100,
                CurrentPrice = 100,
                Status = AuctionStatus.Active,
                Title = "Test Auction",
                Description = "Test Description"
            };
            context.Auctions.Add(auction);

            await context.SaveChangesAsync();

            // Добавляем несколько ставок
            context.Bids.Add(new Bid { Amount = 100, AuctionId = auction.Id, UserId = "user1" });
            context.Bids.Add(new Bid { Amount = 150, AuctionId = auction.Id, UserId = "user1" });
            await context.SaveChangesAsync();

            var result = await controller.GetBids();

            var bids = Assert.IsAssignableFrom<List<Bid>>(result.Value);
            Assert.Equal(2, bids.Count);
        }

        [Fact]
        public async Task GetBid_ExistingId_ReturnsBid()
        {
            var (controller, context) = CreateController();
            // 1. Добавляем пользователя в базу
            var user = new ApplicationUser { Id = "user1", UserName = "testuser@example.com" };
            context.Users.Add(user);

            // 2. Добавляем аукцион, чтобы был валидный AuctionId
            var auction = new Auction
            {
                OwnerId = user.Id,
                StartPrice = 100,
                CurrentPrice = 100,
                Status = AuctionStatus.Active,
                Title = "Test Auction",
                Description = "Test Description"
            };
            context.Auctions.Add(auction);

            await context.SaveChangesAsync();

            var bid = new Bid { Amount = 200, AuctionId = auction.Id, UserId = "user1" };
            context.Bids.Add(bid);
            await context.SaveChangesAsync();

            var result = await controller.GetBid(bid.Id);

            var returnedBid = Assert.IsType<Bid>(result.Value);
            Assert.Equal(bid.Id, returnedBid.Id);
        }

        [Fact]
        public async Task GetBid_NonExistingId_ReturnsNotFound()
        {
            var (controller, _) = CreateController();

            var result = await controller.GetBid(999);

            Assert.IsType<NotFoundResult>(result.Result);
        }


        [Fact]
        public async Task PostBid_ReturnsCreated()
        {
            var (controller, context) = CreateController();

            // 1. Добавляем пользователя в базу
            var user = new ApplicationUser { Id = "user1", UserName = "testuser@example.com" };
            context.Users.Add(user);

            // 2. Добавляем аукцион, чтобы был валидный AuctionId
            var auction = new Auction
            {
                OwnerId = user.Id,
                StartPrice = 100,
                CurrentPrice = 100,
                Status = AuctionStatus.Active,
                Title = "Test Auction",
                Description = "Test Description"
            };
            context.Auctions.Add(auction);

            await context.SaveChangesAsync();

            // 3. Создаём ставку с правильными UserId и AuctionId
            var bid = new Bid
            {
                Amount = 150,
                UserId = user.Id,
                AuctionId = auction.Id
            };

            var result = await controller.PostBid(bid);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal("GetBid", createdResult.ActionName);
        }

        [Fact]
        public async Task PutBid_ValidUpdate_ReturnsNoContent()
        {
            var (controller, context) = CreateController();
            // 1. Добавляем пользователя в базу
            var user = new ApplicationUser { Id = "user1", UserName = "testuser@example.com" };
            context.Users.Add(user);

            // 2. Добавляем аукцион, чтобы был валидный AuctionId
            var auction = new Auction
            {
                OwnerId = user.Id,
                StartPrice = 100,
                CurrentPrice = 100,
                Status = AuctionStatus.Active,
                Title = "Test Auction",
                Description = "Test Description"
            };
            context.Auctions.Add(auction);

            await context.SaveChangesAsync();

            var bid = new Bid { Amount = 100, AuctionId = auction.Id, UserId = "user1" };
            context.Bids.Add(bid);
            await context.SaveChangesAsync();

            bid.Amount = 200;

            var result = await controller.PutBid(bid.Id, bid);

            Assert.IsType<NoContentResult>(result);
            var updatedBid = await context.Bids.FindAsync(bid.Id);
            Assert.Equal(200, updatedBid.Amount);
        }

        [Fact]
        public async Task PutBid_IdMismatch_ReturnsBadRequest()
        {
            var (controller, _) = CreateController();

            var bid = new Bid { Id = 1, Amount = 100, AuctionId = 1, UserId = "user1" };

            var result = await controller.PutBid(2, bid);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task PutBid_NonExistingBid_ReturnsNotFound()
        {
            var (controller, _) = CreateController();

            var bid = new Bid { Id = 999, Amount = 100, AuctionId = 1, UserId = "user1" };

            var result = await controller.PutBid(bid.Id, bid);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteBid_ExistingId_ReturnsNoContent()
        {
            var (controller, context) = CreateController();

            var user = new ApplicationUser { Id = "user1", UserName = "testuser@example.com" };
             context.Users.Add(user);

            
            var auction = new Auction
            {
                OwnerId = user.Id,
                StartPrice = 100,
                CurrentPrice = 100,
                Status = AuctionStatus.Active,
                Title = "Test Auction",
                Description = "Test Description"
            };
            context.Auctions.Add(auction);

            await context.SaveChangesAsync();

           
            var bid = new Bid { Amount = 100, AuctionId = auction.Id, UserId = user.Id };
            context.Bids.Add(bid);
            await context.SaveChangesAsync();

            var result = await controller.DeleteBid(bid.Id);

            Assert.IsType<NoContentResult>(result);
            var deletedBid = await context.Bids.FindAsync(bid.Id);
            Assert.Null(deletedBid);
        }

        [Fact]
        public async Task DeleteBid_NonExistingId_ReturnsNotFound()
        {
            var (controller, _) = CreateController();

            var result = await controller.DeleteBid(999);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
