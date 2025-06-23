using AuctionApp.Data;
using AuctionApp.Models;
using AuctionApp.Services;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Xunit;

namespace AuctionApp.Tests.Services
{
    public class AuctionServiceTests
    {
        private AppDbContext CreateContext() => TestDbContextFactory.Create();

        [Fact]
        public async Task PlaceBidAsync_AuctionNotFound_ReturnsError()
        {
            using var context = CreateContext();
            var service = new AuctionService(context);

            var result = await service.PlaceBidAsync(999, "user1", 100);

            Assert.False(result.Success);
            Assert.Equal("Аукцион не найден", result.Error);
        }

        [Fact]
        public async Task PlaceBidAsync_AuctionNotActive_ReturnsError()
        {
            using var context = CreateContext();
            var auction = new Auction
            {
                Title = "A1", Description = "test",
                Status = AuctionStatus.Sold,
                CurrentPrice = 100,
                StartPrice = 50
            };
            context.Auctions.Add(auction);
            await context.SaveChangesAsync();

            var service = new AuctionService(context);

            var result = await service.PlaceBidAsync(auction.Id, "user1", 150);

            Assert.False(result.Success);
            Assert.Equal("Аукцион не активен", result.Error);
        }

        [Theory]
        [InlineData(100, 100)]
        [InlineData(90, 100)]
        [InlineData(40, 50)]
        public async Task PlaceBidAsync_BidTooLow_ReturnsError(decimal bidAmount, decimal currentPrice)
        {
            using var context = CreateContext();
            var auction = new Auction
            {
                Title = "A1", Description = "test",
                Status = AuctionStatus.Active,
                CurrentPrice = currentPrice,
                StartPrice = 50
            };
            context.Auctions.Add(auction);
            await context.SaveChangesAsync();

            var service = new AuctionService(context);

            var result = await service.PlaceBidAsync(auction.Id, "user1", bidAmount);

            Assert.False(result.Success);
            Assert.Equal("Ставка должна быть выше текущей цены", result.Error);
        }

        [Fact]
        public async Task PlaceBidAsync_ValidBid_UpdatesAuctionAndAddsBid()
        {
            using var context = CreateContext();
             // 1. Добавляем пользователя в базу
            var user = new ApplicationUser { Id = "user1", UserName = "testuser@example.com" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var auction = new Auction
            {
                Title = "A1", Description = "test",
                Status = AuctionStatus.Active,
                CurrentPrice = 100,
                StartPrice = 50
            };
            context.Auctions.Add(auction);
            await context.SaveChangesAsync();

            var service = new AuctionService(context);

            var result = await service.PlaceBidAsync(auction.Id, "user1", 150);

            Assert.True(result.Success);
            Assert.Null(result.Error);

            var updatedAuction = await context.Auctions.FindAsync(auction.Id);
            Assert.Equal(150, updatedAuction.CurrentPrice);
            Assert.Equal("user1", updatedAuction.WinnerId);

            var bid = await context.Bids.FirstOrDefaultAsync(b => b.AuctionId == auction.Id && b.UserId == "user1");
            Assert.NotNull(bid);
            Assert.Equal(150, bid.Amount);
        }

        [Fact]
        public async Task GetAuctionAsync_ExistingAuction_ReturnsAuction()
        {
            using var context = CreateContext();
            var auction = new Auction
            {
                Title = "A1", Description = "test",
                Status = AuctionStatus.Active,
                CurrentPrice = 100,
                StartPrice = 50
            };
            context.Auctions.Add(auction);
            await context.SaveChangesAsync();

            var service = new AuctionService(context);

            var result = await service.GetAuctionAsync(auction.Id);

            Assert.NotNull(result);
            Assert.Equal(auction.Id, result.Id);
        }

        [Fact]
        public async Task GetAuctionAsync_NonExistingAuction_ReturnsNull()
        {
            using var context = CreateContext();
            var service = new AuctionService(context);

            var result = await service.GetAuctionAsync(999);

            Assert.Null(result);
        }
    }
}
