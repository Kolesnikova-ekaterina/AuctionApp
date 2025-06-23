using AuctionApp.Models;
using Xunit;

namespace AuctionApp.Tests.Models
{
    public class ApplicationUserModelTests
    {
        [Fact]
        public void ApplicationUser_DefaultCollections_ShouldBeInitialized()
        {
            var user = new ApplicationUser();

            Assert.NotNull(user.Bids);
            Assert.Empty(user.Bids);

            Assert.NotNull(user.WinnedAuctions);
            Assert.Empty(user.WinnedAuctions);
        }
    }
}
