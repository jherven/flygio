using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flygio.Migrations
{
    /// <inheritdoc />
    public partial class AddAffiliateClickTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LinkType",
                table: "AffiliateClicks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourcePage",
                table: "AffiliateClicks",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AffiliateClicks_LinkType",
                table: "AffiliateClicks",
                column: "LinkType");

            migrationBuilder.CreateIndex(
                name: "IX_AffiliateClicks_SourcePage",
                table: "AffiliateClicks",
                column: "SourcePage");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AffiliateClicks_LinkType",
                table: "AffiliateClicks");

            migrationBuilder.DropIndex(
                name: "IX_AffiliateClicks_SourcePage",
                table: "AffiliateClicks");

            migrationBuilder.DropColumn(
                name: "LinkType",
                table: "AffiliateClicks");

            migrationBuilder.DropColumn(
                name: "SourcePage",
                table: "AffiliateClicks");
        }
    }
}
