using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Wathiq.Documents.Documents;
using Wathiq.Documents.DocumentTypes;
using Wathiq.Documents.Holders;

namespace Wathiq.Documents.EntityFrameworkCore;

// One DbContext per module (ADR-001). Compare the host's WathiqDbContext, which holds every
// ABP module's DbSets: this one will only ever know documents.* tables, so a join to
// reminders.* or AbpUsers is impossible to write, not merely discouraged.
[ConnectionStringName(DocumentsDbProperties.ConnectionStringName)]
public class DocumentsDbContext : AbpDbContext<DocumentsDbContext>
{
    public DbSet<DocumentType> DocumentTypes { get; set; } = default!;
    public DbSet<Holder> Holders { get; set; } = default!;
    public DbSet<Document> Documents { get; set; } = default!;
    // No DbSet<Attachment>: it is reached through Document only (aggregate boundary).
    // ExtractionResult IS its own aggregate (queried and concluded independently of Document).
    public DbSet<Extraction.ExtractionResult> ExtractionResults { get; set; } = default!;

    public DocumentsDbContext(DbContextOptions<DocumentsDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema(DocumentsDbProperties.DbSchema);

        builder.ApplyConfigurationsFromAssembly(typeof(DocumentsDbContext).Assembly);
    }
}
