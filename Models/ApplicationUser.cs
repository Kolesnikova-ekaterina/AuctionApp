using Microsoft.AspNetCore.Identity;

namespace AuctionApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<Bid> Bids { get; set; } = new List<Bid>();
        public ICollection<Auction> WinnedAuctions { get; set; } = new List<Auction>();
    }
}
