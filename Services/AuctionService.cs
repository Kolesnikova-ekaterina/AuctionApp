using AuctionApp.Data;
using AuctionApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace AuctionApp.Services
{
    public class AuctionService : IAuctionService
    {
        private readonly AppDbContext _context;

        public AuctionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string Error)> PlaceBidAsync(int auctionId, string userId, decimal amount)
        {
            var auction = await _context.Auctions.FirstOrDefaultAsync(a => a.Id == auctionId);

            if (auction == null)
                return (false, "Аукцион не найден");

            if (auction.Status != AuctionStatus.Active)
                return (false, "Аукцион не активен");

            if (amount <= auction.CurrentPrice || amount < auction.StartPrice)
                return (false, "Ставка должна быть выше текущей цены");

            auction.CurrentPrice = amount;
            auction.WinnerId = userId;

            Bid bid = new Bid();
            bid.Amount = amount;
            bid.UserId = userId;
            bid.AuctionId = auctionId;
            _context.Bids.Add(bid);

            await _context.SaveChangesAsync();

            return (true, null);
        }

        public async Task<Auction> GetAuctionAsync(int auctionId)
        {
            return await _context.Auctions.FindAsync(auctionId);
        }

        /*public async Task CompleteAuctionAsync(int auctionId)
        {
            var auction = await _context.Auctions.FindAsync(auctionId);
            if (auction == null) return;

            auction.Status = AuctionStatus.Sold;
            await _context.SaveChangesAsync();
        }*/
    }
}
