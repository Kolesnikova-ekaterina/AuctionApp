using System.ComponentModel.DataAnnotations;
using AuctionApp.Models;
using Xunit;

namespace AuctionApp.Tests.Models
{
    public class BidModelTests
    {
        [Fact]
        public void Bid_RequiredProperties_ShouldBeValid()
        {
            var bid = new Bid
            {
                AuctionId = 1,
                UserId = "user1",
                Amount = 150m
            };

            var validationResults = ValidateModel(bid);
            Assert.Empty(validationResults);
        }

        private static ValidationResult[] ValidateModel(object model)
        {
            var context = new ValidationContext(model, null, null);
            var results = new System.Collections.Generic.List<ValidationResult>();
            Validator.TryValidateObject(model, context, results, true);
            return results.ToArray();
        }
    }
}
