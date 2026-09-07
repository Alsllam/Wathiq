using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wathiq.Guides.Migrations
{
    /// <inheritdoc />
    public partial class AddGuideFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GuideFeedback",
                schema: "guides",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GuideVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Kind = table.Column<byte>(type: "tinyint", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuideFeedback", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuideFeedback_GuideVersion_GuideVersionId",
                        column: x => x.GuideVersionId,
                        principalSchema: "guides",
                        principalTable: "GuideVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuideFeedback_GuideVersionId",
                schema: "guides",
                table: "GuideFeedback",
                column: "GuideVersionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuideFeedback",
                schema: "guides");
        }
    }
}
