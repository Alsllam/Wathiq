using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wathiq.Reminders.Migrations
{
    /// <inheritdoc />
    public partial class AddReminderRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "reminders");

            migrationBuilder.CreateTable(
                name: "ReminderRule",
                schema: "reminders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OffsetsDays = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Channels = table.Column<byte>(type: "tinyint", nullable: false),
                    QuietFrom = table.Column<TimeOnly>(type: "time", nullable: true),
                    QuietTo = table.Column<TimeOnly>(type: "time", nullable: true),
                    TimeZoneId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReminderRule", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_ReminderRule_UserId",
                schema: "reminders",
                table: "ReminderRule",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReminderRule",
                schema: "reminders");
        }
    }
}
