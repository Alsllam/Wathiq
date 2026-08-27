using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wathiq.Documents.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentAndAttachment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Document",
                schema: "documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HolderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    IssueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    PreviousExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
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
                    table.PrimaryKey("PK_Document", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Document_DocumentType_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalSchema: "documents",
                        principalTable: "DocumentType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Document_Holder_HolderId",
                        column: x => x.HolderId,
                        principalSchema: "documents",
                        principalTable: "Holder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Attachment",
                schema: "documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BlobKey = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    OcrText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsEncrypted = table.Column<bool>(type: "bit", nullable: false),
                    Sha256 = table.Column<byte[]>(type: "varbinary(32)", maxLength: 32, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attachment_Document_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "documents",
                        principalTable: "Document",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attachment_DocumentId",
                schema: "documents",
                table: "Attachment",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Document_DocumentTypeId",
                schema: "documents",
                table: "Document",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Document_ExpiryDate",
                schema: "documents",
                table: "Document",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_Document_HolderId",
                schema: "documents",
                table: "Document",
                column: "HolderId");

            migrationBuilder.CreateIndex(
                name: "IX_Document_OwnerUserId",
                schema: "documents",
                table: "Document",
                column: "OwnerUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attachment",
                schema: "documents");

            migrationBuilder.DropTable(
                name: "Document",
                schema: "documents");
        }
    }
}
