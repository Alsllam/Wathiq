using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wathiq.Guides.Migrations
{
    /// <inheritdoc />
    public partial class AddGuideChunk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GuideChunk",
                schema: "guides",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GuideVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChunkNo = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Embedding = table.Column<byte[]>(type: "varbinary(4096)", maxLength: 4096, nullable: false),
                    EmbeddingModel = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    TokenCount = table.Column<int>(type: "int", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuideChunk", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuideChunk_GuideVersion_GuideVersionId",
                        column: x => x.GuideVersionId,
                        principalSchema: "guides",
                        principalTable: "GuideVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuideChunk_GuideVersionId",
                schema: "guides",
                table: "GuideChunk",
                column: "GuideVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_GuideChunk_GuideVersionId_ChunkNo",
                schema: "guides",
                table: "GuideChunk",
                columns: new[] { "GuideVersionId", "ChunkNo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuideChunk",
                schema: "guides");
        }
    }
}
