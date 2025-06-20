using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AuctionApp.Models;

namespace AuctionApp.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Bid>(entity =>
            {
            
                entity.HasOne(mc => mc.User)
                    .WithMany(m => m.Bids)
                    .HasForeignKey(mc => mc.UserId)
                    .OnDelete(DeleteBehavior.Cascade); 

                entity.HasOne(mc => mc.Auction)
                    .WithMany(p => p.Bids)
                    .HasForeignKey(mc => mc.AuctionId)
                    .OnDelete(DeleteBehavior.Cascade); 
            });
        }

        public DbSet<Auction> Auctions { get; set; }
        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<Bid> Bids{ get; set; }
    }
}
