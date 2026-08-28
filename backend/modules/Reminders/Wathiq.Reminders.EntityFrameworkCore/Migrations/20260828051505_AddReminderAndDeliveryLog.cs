using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wathiq.Reminders.Migrations
{
    /// <inheritdoc />
    public partial class AddReminderAndDeliveryLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reminder",
                schema: "reminders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OffsetDays = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_Reminder", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryLog",
                schema: "reminders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReminderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Channel = table.Column<byte>(type: "tinyint", nullable: false),
                    AttemptedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Succeeded = table.Column<bool>(type: "bit", nullable: false),
                    Error = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryLog_Reminder_ReminderId",
                        column: x => x.ReminderId,
                        principalSchema: "reminders",
                        principalTable: "Reminder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryLog_ReminderId",
                schema: "reminders",
                table: "DeliveryLog",
                column: "ReminderId");

            migrationBuilder.CreateIndex(
                name: "IX_Reminder_Status_DueDate",
                schema: "reminders",
                table: "Reminder",
                columns: new[] { "Status", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Reminder_UserId",
                schema: "reminders",
                table: "Reminder",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UQ_Reminder_DocumentId_OffsetDays",
                schema: "reminders",
                table: "Reminder",
                columns: new[] { "DocumentId", "OffsetDays" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryLog",
                schema: "reminders");

            migrationBuilder.DropTable(
                name: "Reminder",
                schema: "reminders");
        }
    }
}
