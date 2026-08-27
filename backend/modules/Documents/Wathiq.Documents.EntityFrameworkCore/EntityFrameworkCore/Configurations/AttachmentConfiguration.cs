using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Wathiq.Documents.Documents;

namespace Wathiq.Documents.EntityFrameworkCore.Configurations;

public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> b)
    {
        b.ToTable("Attachment", DocumentsDbProperties.DbSchema);
        b.ConfigureByConvention();

        b.Property(x => x.BlobKey).IsRequired().HasMaxLength(DocumentConsts.MaxBlobKeyLength);
        b.Property(x => x.MimeType).IsRequired().HasMaxLength(DocumentConsts.MaxMimeTypeLength);
        b.Property(x => x.Sha256).IsRequired().HasMaxLength(32);

        b.HasIndex(x => x.DocumentId).HasDatabaseName("IX_Attachment_DocumentId");
    }
}
