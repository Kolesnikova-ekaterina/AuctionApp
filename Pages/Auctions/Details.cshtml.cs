using AuctionApp.Data;
using AuctionApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AuctionApp.Pages.Auctions
{
    public class DetailsModel : PageModel
    {
        public readonly AppDbContext _context;
        public List<Bid> Bids = new List<Bid>();

        public DetailsModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Auction Auction { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Auction = await _context.Auctions.FindAsync(id);
            System.Console.WriteLine( _context.Bids.Include(b=>b.User).Where(b => b.AuctionId == id)  );
            Bids = await _context.Bids.Include(b=>b.User).Where(b => b.AuctionId == id).ToListAsync();
            if (Auction == null)
                return NotFound();

            return Page();
        }
    }
}
