using AuctionApp.Data;
using AuctionApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace AuctionApp.Pages.Auctions
{
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;

        public CreateModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Auction Auction { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            //if (!ModelState.IsValid)
             //   return Page();

            Auction.Status = AuctionStatus.Active;
            Auction.CurrentPrice = Auction.StartPrice;
            Auction.OwnerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            _context.Auctions.Add(Auction);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}
