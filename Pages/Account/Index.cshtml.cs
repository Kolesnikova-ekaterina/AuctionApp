using AuctionApp.Data;
using AuctionApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AuctionApp.Pages.Account
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        [BindProperty(SupportsGet = true)]
        public string id { get; set; } = "";

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<Auction> Auctions { get; set; }
        public IList<Auction> WinnedAuctions { get; set; }
        public IList<ApplicationUser> Sellers { get; set; }

        public async Task OnGetAsync(string id)
        {
            var uid = await _context.Users.Include(u=>u.WinnedAuctions).Where(u => u.UserName == id).FirstAsync();
            Auctions = await _context.Auctions.Where(a=>a.OwnerId == uid.Id).ToListAsync();
            WinnedAuctions = uid.WinnedAuctions.ToList();
            Sellers = await _context.Users.ToListAsync();
        }
        public async Task<IActionResult> OnPostCloseAsync(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var auction = await _context.Auctions.FirstOrDefaultAsync(a => a.Id == id && a.OwnerId == userId);

            if (auction == null)
                return NotFound();

            if (auction.Status != AuctionStatus.Active)
                return BadRequest("Лот нельзя закрыть.");

            if (auction.WinnerId==null)
            {
                auction.Status = AuctionStatus.Closing;
            }
            else
            {
                auction.Status = AuctionStatus.Sold;
                
            }
            var winner = await _context.Users.Where(u => u.Id == auction.WinnerId).FirstAsync();
            winner.WinnedAuctions.Add(auction);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

    }
}
