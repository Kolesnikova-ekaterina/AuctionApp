using AuctionApp.Data;
using AuctionApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AuctionApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuctionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuctionsController(AppDbContext context)
        {
            _context = context;
        }

        private string UserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpPost]
        public async Task<IActionResult> CreateAuction([FromBody] Auction auction)
        {
            auction.OwnerId = UserId();
            auction.Status = AuctionStatus.Active;
            auction.CurrentPrice = auction.StartPrice;

            _context.Auctions.Add(auction);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAuction), new { id = auction.Id }, auction);
        }
        [HttpGet()]
        public async Task<IActionResult> GetAuction()
        {
            var auction = await _context.Auctions.Include(a => a.Bids).ToListAsync();
            if (auction == null) return NotFound();
            return Ok(auction);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAuction(int id)
        {
            var auction = await _context.Auctions.FindAsync(id);
            if (auction == null) return NotFound();
            return Ok(auction);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuction(int id)
        {
            var auction = await _context.Auctions.FirstOrDefaultAsync(a => a.Id == id);
            if (auction == null) return NotFound();

            _context.Auctions.Remove(auction);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("{id}/close")]
        public async Task<IActionResult> CloseAuction(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var auction = await _context.Auctions.FirstOrDefaultAsync(a => a.Id == id);

            if (auction == null)
                return NotFound("Лот не найден или у вас нет прав на его закрытие.");

            if (auction.Status == AuctionStatus.Sold || auction.Status == AuctionStatus.Closing)
                return BadRequest("Лот уже закрыт или в процессе закрытия.");

            auction.Status = AuctionStatus.Closing; // или AuctionStatus.Sold, если хотите сразу закрывать
            // Можно зафиксировать цену закрытия, например, текущую цену
            auction.CurrentPrice = auction.CurrentPrice; // здесь можно добавить поле PriceAtClosing, если нужно

            await _context.SaveChangesAsync();

            return Ok(auction);
        }

        [HttpPatch("{id}/sold")]
        public async Task<IActionResult> MarkSold(int id)
        {
            var auction = await _context.Auctions.FirstOrDefaultAsync(a => a.Id == id && a.OwnerId == UserId());
            if (auction == null) return NotFound();

            auction.Status = AuctionStatus.Sold;

            await _context.SaveChangesAsync();

            return Ok(auction);
        }
    }
}
