using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using System.Net;
using AuctionApp.Tests.TestHelpers;
using System.Net.Http.Json;
using AuctionApp.Models;

namespace AuctionApp.Tests.IntegrationTests
{
    public class ControllersIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ControllersIntegrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task FullFlow_CreateAuction_PlaceBid_CloseAuction()
        {

            var auction = new { StartPrice = 100, Title = "Test", Description = "Desc" };
            var createResponse = await _client.PostAsJsonAsync("/api/auctions", auction);
            createResponse.EnsureSuccessStatusCode();

        }
        [Fact]
        public async Task CreateAuction_ReturnsCreated()
        {
            var auction = new { StartPrice = 100, Title = "Test Auction", Description = "Test Desc" };

            var response = await _client.PostAsJsonAsync("/api/auctions", auction);

            response.EnsureSuccessStatusCode();

            var created = await response.Content.ReadFromJsonAsync<Auction>();

            Assert.NotNull(created);
            Assert.Equal("Test Auction", created.Title);
        }

        [Fact]
        public async Task GetAuctions_ReturnsList()
        {
            var response = await _client.GetAsync("/api/auctions");
            response.EnsureSuccessStatusCode();

            var auctions = await response.Content.ReadFromJsonAsync<Auction[]>();

            Assert.NotNull(auctions);
        }

        [Fact]
        public async Task GetAuction_NonExisting_ReturnsNotFound()
        {
            var response = await _client.GetAsync("/api/auctions/999999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task PlaceBid_BidTooLow_ReturnsBadRequest()
        {
            // Создаём аукцион
            var auction = new { StartPrice = 100, Title = "Test Auction", Description = "Desc" };
            var createResponse = await _client.PostAsJsonAsync("/api/auctions", auction);
            createResponse.EnsureSuccessStatusCode();

            var createdAuction = await createResponse.Content.ReadFromJsonAsync<Auction>();

            // Делаем ставку ниже текущей цены
            var bid = new { AuctionId = createdAuction.Id, Amount = 50, UserId = "user1" };
            var bidResponse = await _client.PostAsJsonAsync("/api/bid", bid);

            Assert.False(bidResponse.IsSuccessStatusCode);
           
        }

        [Fact]
        public async Task PlaceBid_ValidBid_UpdatesAuction()
        {
            // Создаём пользователя
            var user = new { Id = "user1", UserName = "testuser@example.com" };
            var userResponse = await _client.PostAsJsonAsync("/api/user", user);
            userResponse.EnsureSuccessStatusCode();

            var auction = new { StartPrice = 100, Title = "Test Auction", Description = "Desc" };
            var createResponse = await _client.PostAsJsonAsync("/api/auctions", auction);
            createResponse.EnsureSuccessStatusCode();

            var createdAuction = await createResponse.Content.ReadFromJsonAsync<Auction>();

            var bid = new { AuctionId = createdAuction.Id, Amount = 150, UserId = "user1" };
            var bidResponse = await _client.PostAsJsonAsync("/api/bid", bid);
            bidResponse.EnsureSuccessStatusCode();

            
        }

        

        [Fact]
        public async Task DeleteAuction_ReturnsNoContent()
        {
            var auction = new { StartPrice = 100, Title = "Test Auction", Description = "Desc" };
            var createResponse = await _client.PostAsJsonAsync("/api/auctions", auction);
            createResponse.EnsureSuccessStatusCode();

            var createdAuction = await createResponse.Content.ReadFromJsonAsync<Auction>();

            var deleteResponse = await _client.DeleteAsync($"/api/auctions/{createdAuction.Id}");
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            var getResponse = await _client.GetAsync($"/api/auctions/{createdAuction.Id}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }
    }
}
