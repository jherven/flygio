using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flygio.Migrations
{
    /// <inheritdoc />
    public partial class AddStripeSubscriptionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StripeCustomerId",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeSubscriptionId",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubscriptionCurrentPeriodEnd",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubscriptionStatus",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_StripeCustomerId",
                table: "Users",
                column: "StripeCustomerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_StripeCustomerId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "StripeCustomerId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "StripeSubscriptionId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SubscriptionCurrentPeriodEnd",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SubscriptionStatus",
                table: "Users");
        }
    }
}
