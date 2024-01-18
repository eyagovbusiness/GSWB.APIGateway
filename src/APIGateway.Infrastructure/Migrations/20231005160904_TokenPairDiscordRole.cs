using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIGateway.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TokenPairDiscordRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DiscordRoleId",
                table: "TokenPairAuthRecords",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscordRoleId",
                table: "TokenPairAuthRecords");
        }
    }
}
