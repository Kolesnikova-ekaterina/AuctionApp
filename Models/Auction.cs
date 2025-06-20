using System.ComponentModel.DataAnnotations;

namespace AuctionApp.Models
{
    public enum AuctionStatus
    {
        Active,
        Closing,
        Sold
    }

    public class Auction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public decimal StartPrice { get; set; }

        public decimal CurrentPrice { get; set; }

        public AuctionStatus Status { get; set; }

        public string? OwnerId { get; set; }

        public string? WinnerId { get; set; }

        public DateTime? EndTime { get; set; }

        public ICollection<Bid> Bids { get; set; } = new List<Bid>();
    }
}
