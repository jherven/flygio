using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Flygio.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SearchEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OriginCode = table.Column<string>(type: "text", nullable: false),
                    DestinationCode = table.Column<string>(type: "text", nullable: false),
                    DepartureDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Passengers = table.Column<int>(type: "integer", nullable: false),
                    ResultCount = table.Column<int>(type: "integer", nullable: false),
                    SessionId = table.Column<string>(type: "text", nullable: true),
                    SearchedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SearchEvents_OriginCode_DestinationCode",
                table: "SearchEvents",
                columns: new[] { "OriginCode", "DestinationCode" });

            migrationBuilder.CreateIndex(
                name: "IX_SearchEvents_SearchedAt",
                table: "SearchEvents",
                column: "SearchedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SearchEvents");
        }
    }
}
