using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIGateway.Infrastructure.Migrations.AuthDb
{
    /// <inheritdoc />
    public partial class TokenMemberId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscordUserId",
                table: "TokenPairAuthRecords");

            migrationBuilder.AddColumn<Guid>(
                name: "MemberId",
                table: "TokenPairAuthRecords",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MemberId",
                table: "TokenPairAuthRecords");

            migrationBuilder.AddColumn<decimal>(
                name: "DiscordUserId",
                table: "TokenPairAuthRecords",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
