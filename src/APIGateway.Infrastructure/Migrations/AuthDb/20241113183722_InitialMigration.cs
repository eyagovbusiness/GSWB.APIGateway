﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIGateway.Infrastructure.Migrations.AuthDb
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TokenPairAuthRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId_GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    MemberId_UserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    RoleId_GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    RoleId_RoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    ExpiryDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RefreshToken = table.Column<string>(type: "character varying(88)", maxLength: 88, nullable: false),
                    AccessToken = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    IsOutdated = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenPairAuthRecords", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TokenPairAuthRecords");
        }
    }
}
