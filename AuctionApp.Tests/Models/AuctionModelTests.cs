using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AuctionApp.Models;
using Xunit;
using System.Net.Http.Json;

namespace AuctionApp.Tests.Models
{
    public class AuctionModelTests
    {
        [Fact]
        public void Auction_RequiredProperties_ShouldBeValid()
        {
            var auction = new Auction
            {
                Title = "Test Auction",
                StartPrice = 100m,
                CurrentPrice = 100m,
                Status = AuctionStatus.Active,
                OwnerId = "owner1"
            };

            var validationResults = ValidateModel(auction);
            Assert.Empty(validationResults);
        }

        [Fact]
        public void Auction_MissingRequiredProperties_ShouldFailValidation()
        {
            var auction = new Auction();

            var validationResults = ValidateModel(auction);

            Assert.Contains(validationResults, v => v.ErrorMessage.Contains("required"));
            Assert.Contains(validationResults, v => v.ErrorMessage.Contains("required"));
        }

        [Fact]
        public void Auction_DefaultValues_ShouldBeCorrect()
        {
            var auction = new Auction();

            Assert.Equal(AuctionStatus.Active, auction.Status);
            Assert.NotNull(auction.Bids);
            Assert.Empty(auction.Bids);
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
