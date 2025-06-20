using AuctionApp.Models;
using System.Threading.Tasks;

namespace AuctionApp.Services
{
    public interface IAuctionService
    {
        Task<(bool Success, string Error)> PlaceBidAsync(int auctionId, string userId, decimal amount);
        Task<Auction> GetAuctionAsync(int auctionId);
       // Task CompleteAuctionAsync(int auctionId);
    }
}
