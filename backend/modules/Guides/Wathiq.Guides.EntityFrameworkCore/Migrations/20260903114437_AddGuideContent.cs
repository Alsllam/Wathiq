using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wathiq.Guides.Migrations
{
    /// <inheritdoc />
    public partial class AddGuideContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "guides");

            migrationBuilder.CreateTable(
                name: "Guide",
                schema: "guides",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    TitleAr = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    TitleEn = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PublishedVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_Guide", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GuideVersion",
                schema: "guides",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GuideId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionNo = table.Column<int>(type: "int", nullable: false),
                    Language = table.Column<string>(type: "nchar(2)", fixedLength: true, maxLength: 2, nullable: false),
                    BodyMarkdown = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredDocuments = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Fees = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    LastVerifiedAt = table.Column<DateOnly>(type: "date", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_GuideVersion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuideVersion_Guide_GuideId",
                        column: x => x.GuideId,
                        principalSchema: "guides",
                        principalTable: "Guide",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuideStep",
                schema: "guides",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GuideVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StepNo = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuideStep", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuideStep_GuideVersion_GuideVersionId",
                        column: x => x.GuideVersionId,
                        principalSchema: "guides",
                        principalTable: "GuideVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Guide_PublishedVersionId",
                schema: "guides",
                table: "Guide",
                column: "PublishedVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Guide_Slug",
                schema: "guides",
                table: "Guide",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GuideStep_GuideVersionId_StepNo",
                schema: "guides",
                table: "GuideStep",
                columns: new[] { "GuideVersionId", "StepNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GuideVersion_GuideId",
                schema: "guides",
                table: "GuideVersion",
                column: "GuideId");

            migrationBuilder.CreateIndex(
                name: "IX_GuideVersion_GuideId_VersionNo",
                schema: "guides",
                table: "GuideVersion",
                columns: new[] { "GuideId", "VersionNo" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Guide_GuideVersion_PublishedVersionId",
                schema: "guides",
                table: "Guide",
                column: "PublishedVersionId",
                principalSchema: "guides",
                principalTable: "GuideVersion",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Guide_GuideVersion_PublishedVersionId",
                schema: "guides",
                table: "Guide");

            migrationBuilder.DropTable(
                name: "GuideStep",
                schema: "guides");

            migrationBuilder.DropTable(
                name: "GuideVersion",
                schema: "guides");

            migrationBuilder.DropTable(
                name: "Guide",
                schema: "guides");
        }
    }
}
