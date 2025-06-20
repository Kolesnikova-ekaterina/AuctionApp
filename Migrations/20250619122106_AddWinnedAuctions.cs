using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuctionApp.Migrations
{
    /// <inheritdoc />
    public partial class AddWinnedAuctions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Auctions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Auctions_ApplicationUserId",
                table: "Auctions",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Auctions_AspNetUsers_ApplicationUserId",
                table: "Auctions",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Auctions_AspNetUsers_ApplicationUserId",
                table: "Auctions");

            migrationBuilder.DropIndex(
                name: "IX_Auctions_ApplicationUserId",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Auctions");
        }
    }
}
