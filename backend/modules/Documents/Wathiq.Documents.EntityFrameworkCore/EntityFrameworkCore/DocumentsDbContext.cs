using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace Wathiq.Documents.EntityFrameworkCore;

// One DbContext per module (ADR-001). Compare the host's WathiqDbContext, which holds every
// ABP module's DbSets: this one will only ever know documents.* tables, so a join to
// reminders.* or AbpUsers is impossible to write, not merely discouraged.
[ConnectionStringName(DocumentsDbProperties.ConnectionStringName)]
public class DocumentsDbContext : AbpDbContext<DocumentsDbContext>
{
    public DocumentsDbContext(DbContextOptions<DocumentsDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema(DocumentsDbProperties.DbSchema);

        // Entity configurations are added per step (1.4 DocumentType/Holder, 1.5 Document...).
        builder.ApplyConfigurationsFromAssembly(typeof(DocumentsDbContext).Assembly);
    }
}
