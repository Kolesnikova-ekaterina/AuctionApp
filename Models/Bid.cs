using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AuctionApp.Models
{
    public class Bid
    {
        [Key]
        public int Id { get; set; }

        public int AuctionId { get; set; }
        public string UserId { get; set; }
        public decimal Amount{ get; set;}

        [JsonIgnore]
        public Auction? Auction { get; set; } = null;
        
        [JsonIgnore]
        public ApplicationUser? User { get; set; } = null ;
    }
}