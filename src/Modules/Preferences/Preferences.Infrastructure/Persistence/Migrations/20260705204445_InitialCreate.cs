using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Preferences.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "preferences",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipient_address = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    channel = table.Column<int>(type: "integer", nullable: false),
                    is_opted_out = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_preferences", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_preferences_tenant_recipient_channel",
                table: "preferences",
                columns: new[] { "tenant_id", "recipient_address", "channel" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "preferences");
        }
    }
}
