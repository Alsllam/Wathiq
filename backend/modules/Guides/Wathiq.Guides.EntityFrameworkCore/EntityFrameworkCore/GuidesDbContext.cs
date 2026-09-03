using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace Wathiq.Guides.EntityFrameworkCore;

// Fifth module DbContext (ADR-001): only guides.* is visible from here.
[ConnectionStringName(GuidesDbProperties.ConnectionStringName)]
public class GuidesDbContext : AbpDbContext<GuidesDbContext>
{
    public DbSet<Guides.Guide> Guides { get; set; } = default!;
    public DbSet<Guides.GuideVersion> GuideVersions { get; set; } = default!;
    public DbSet<Guides.GuideChunk> GuideChunks { get; set; } = default!;
    // GuideStep rides the GuideVersion aggregate; GuideFeedback arrives with 5.6.

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
