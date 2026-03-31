using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Flygio.Migrations
{
    /// <inheritdoc />
    public partial class AddAffiliateClicks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AffiliateClicks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Provider = table.Column<string>(type: "text", nullable: false),
                    OriginCode = table.Column<string>(type: "text", nullable: false),
                    DestinationCode = table.Column<string>(type: "text", nullable: false),
                    SubId = table.Column<string>(type: "text", nullable: false),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    Referer = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    ClickedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AffiliateClicks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AffiliateClicks_ClickedAt",
                table: "AffiliateClicks",
                column: "ClickedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AffiliateClicks_Provider_SubId",
                table: "AffiliateClicks",
                columns: new[] { "Provider", "SubId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AffiliateClicks");
        }
    }
}
