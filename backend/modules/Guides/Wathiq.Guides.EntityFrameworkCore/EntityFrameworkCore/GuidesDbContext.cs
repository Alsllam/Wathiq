using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace Wathiq.Guides.EntityFrameworkCore;

// Fifth module DbContext (ADR-001): only guides.* is visible from here.
[ConnectionStringName(GuidesDbProperties.ConnectionStringName)]
public class GuidesDbContext : AbpDbContext<GuidesDbContext>
{
    // Guide / GuideVersion / GuideStep arrive with 5.2, GuideChunk with 5.3, GuideFeedback with 5.6.

    public GuidesDbContext(DbContextOptions<GuidesDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema(GuidesDbProperties.DbSchema);

        builder.ApplyConfigurationsFromAssembly(typeof(GuidesDbContext).Assembly);
    }
}
