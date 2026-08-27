using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Wathiq.Documents.Documents;

namespace Wathiq.Documents.EntityFrameworkCore.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> b)
    {
        b.ToTable("Document", DocumentsDbProperties.DbSchema);
        b.ConfigureByConvention();

        b.Property(x => x.Number).HasMaxLength(DocumentConsts.MaxNumberLength);
        b.Property(x => x.Notes).HasMaxLength(DocumentConsts.MaxNotesLength);
        b.Property(x => x.Status).HasConversion<byte>();

        // Owned type: the ValidityPeriod value object flattens into the two columns database.md
        // specifies. No separate table, no Id - it is part of the Document row.
        b.OwnsOne(x => x.Validity, v =>
        {
            v.Property(p => p.IssueDate).HasColumnName("IssueDate");
            v.Property(p => p.ExpiryDate).HasColumnName("ExpiryDate");
            // Timeline query (NFR-PRF-001) lives here because the column belongs to the owned type.
            v.HasIndex("ExpiryDate");
        });
        b.Navigation(x => x.Validity).IsRequired();

        b.HasIndex(x => x.OwnerUserId).HasDatabaseName("IX_Document_OwnerUserId");
        b.HasIndex(x => x.HolderId).HasDatabaseName("IX_Document_HolderId");

        // Both are FKs *inside* this schema (Holder, DocumentType) - allowed by DB2.
        b.HasOne<Holders.Holder>().WithMany().HasForeignKey(x => x.HolderId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<DocumentTypes.DocumentType>().WithMany().HasForeignKey(x => x.DocumentTypeId).OnDelete(DeleteBehavior.Restrict);

        // Aggregate-internal collection: EF loads it with the root (AutoInclude) and cascades deletes.
        b.HasMany(x => x.Attachments).WithOne().HasForeignKey(a => a.DocumentId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Attachments).AutoInclude();
    }
}
