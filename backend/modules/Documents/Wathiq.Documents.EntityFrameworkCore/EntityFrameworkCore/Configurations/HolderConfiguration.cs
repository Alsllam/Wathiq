using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Wathiq.Documents.Holders;

namespace Wathiq.Documents.EntityFrameworkCore.Configurations;

public class HolderConfiguration : IEntityTypeConfiguration<Holder>
{
    public void Configure(EntityTypeBuilder<Holder> b)
    {
        b.ToTable("Holder", DocumentsDbProperties.DbSchema);
        b.ConfigureByConvention();

        b.Property(x => x.FullName).IsRequired().HasMaxLength(HolderConsts.MaxFullNameLength);
        // Enum stored as its numeric value; declaring the CLR type as byte gives tinyint per database.md.
        b.Property(x => x.Relation).HasConversion<byte>();

        b.HasIndex(x => x.UserId).HasDatabaseName("IX_Holder_UserId");
        // "Exactly one self-holder per user" as a database rule, not just a HolderManager rule.
        b.HasIndex(x => x.UserId).IsUnique().HasFilter("[IsSelf] = 1").HasDatabaseName("UQ_Holder_UserId_IsSelf");
    }
}
