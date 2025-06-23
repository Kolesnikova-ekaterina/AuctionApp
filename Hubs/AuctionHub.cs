using AuctionApp.Models;
using AuctionApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace AuctionApp.Hubs
{
    [Authorize]
    public class AuctionHub : Hub
    {
        private readonly IAuctionService _auctionService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuctionHub(IAuctionService auctionService, UserManager<ApplicationUser> userManager)
        {
            _auctionService = auctionService;
            _userManager = userManager;
        }

        public async Task JoinAuctionGroup(int auctionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"auction-{auctionId}");
            await Clients.OthersInGroup($"auction-{auctionId}")
            .SendAsync("NotifyD", $"{Context.User.Identity.Name} присоединился к аукциону");
        }

        public async Task SendAlert(string message)
        {
            var user = Context.User.Identity.Name;
            var formattedMessage = $"[{user}]: {message}";

            await Clients.Others.SendAsync("ReceiveAlert", formattedMessage);
        }
        public async Task AddNewAuction()
        {
            

            await Clients.Others.SendAsync("AddNewAuction_recieve");
        }

        public async Task PlaceBid(int auctionId, decimal amount)
        {
            //System.Console.WriteLine(Context.UserIdentifier);
            var auction = await _auctionService.GetAuctionAsync(auctionId);
            var WinnerId = auction.WinnerId;
            var user = await _userManager.GetUserAsync(Context.User);
            var result = await _auctionService.PlaceBidAsync(auctionId, user.Id, amount);
            //System.Console.WriteLine(result);

            if (result.Success)
            {
                
                await Clients.All.SendAsync("BidPlaced", $"Пользователь {user.UserName} сделал ставку: {amount}", amount);
                
                if (WinnerId!=null){
                    await Clients.User(WinnerId).SendAsync("ReceiveAlert", $"Пользователь {user.UserName} сделал ставку на аукционе #{auction.Title}");
                }
                
                //var auction = await _auctionService.GetAuctionAsync(auctionId);
                if (auction != null)
                {
                    await Clients.User(auction.OwnerId)
                        .SendAsync("Notify", $"На ваш лот '{auction.Title}' сделана новая ставка: {amount} от {user.UserName}");
                }
            }
            else
            {
                await Clients.Caller.SendAsync("Error", result.Error);
            }
        }
        
        public async Task NotifyOwner(int auctionId, string message)
        {
           
            var auction = await _auctionService.GetAuctionAsync(auctionId);
            if (auction == null)
                return;

            var ownerId = auction.OwnerId; 

            await Clients.User(ownerId).SendAsync("Notify", message);
        }
            }
}
