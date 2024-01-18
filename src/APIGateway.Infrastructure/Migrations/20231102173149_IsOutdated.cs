using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIGateway.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IsOutdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsRevoked",
                table: "TokenPairAuthRecords",
                newName: "IsOutdated");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsOutdated",
                table: "TokenPairAuthRecords",
                newName: "IsRevoked");
        }
    }
}
