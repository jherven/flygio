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
            // Intentionally empty — LinkType/SourcePage columns removed from model
            // to avoid ALTER TABLE on AffiliateClicks (DB ownership permission issue)
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
