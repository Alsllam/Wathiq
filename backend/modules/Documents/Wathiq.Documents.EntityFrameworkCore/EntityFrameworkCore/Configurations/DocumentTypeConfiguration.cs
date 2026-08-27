using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Wathiq.Documents.DocumentTypes;

namespace Wathiq.Documents.EntityFrameworkCore.Configurations;

public class DocumentTypeConfiguration : IEntityTypeConfiguration<DocumentType>
{
    public void Configure(EntityTypeBuilder<DocumentType> b)
    {
        b.ToTable("DocumentType", DocumentsDbProperties.DbSchema);
        // Maps the ABP base-class columns (audit, soft delete, ConcurrencyStamp, ExtraProperties) by convention.
        b.ConfigureByConvention();

        b.Property(x => x.Code).IsRequired().HasMaxLength(DocumentTypeConsts.MaxCodeLength);
        b.Property(x => x.NameAr).IsRequired().HasMaxLength(DocumentTypeConsts.MaxNameLength);
        b.Property(x => x.NameEn).IsRequired().HasMaxLength(DocumentTypeConsts.MaxNameLength);

        b.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_DocumentType_Code");
    }
}
