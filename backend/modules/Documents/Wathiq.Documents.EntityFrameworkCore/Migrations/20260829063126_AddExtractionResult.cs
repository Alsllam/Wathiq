using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wathiq.Documents.Migrations
{
    /// <inheritdoc />
    public partial class AddExtractionResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExtractionResult",
                schema: "documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttachmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Model = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    PromptVersion = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    RawJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Confidence = table.Column<decimal>(type: "decimal(4,3)", precision: 4, scale: 3, nullable: true),
                    Outcome = table.Column<byte>(type: "tinyint", nullable: false),
                    DurationMs = table.Column<int>(type: "int", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtractionResult", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExtractionResult_Attachment_AttachmentId",
                        column: x => x.AttachmentId,
                        principalSchema: "documents",
                        principalTable: "Attachment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExtractionResult_AttachmentId",
                schema: "documents",
                table: "ExtractionResult",
                column: "AttachmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExtractionResult",
                schema: "documents");
        }
    }
}
