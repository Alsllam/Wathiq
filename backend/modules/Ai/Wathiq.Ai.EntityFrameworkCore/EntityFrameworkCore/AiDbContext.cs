using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Wathiq.Ai.Usage;

namespace Wathiq.Ai.EntityFrameworkCore;

// Third module DbContext (ADR-001): only ai.* is visible from here.
[ConnectionStringName(AiDbProperties.ConnectionStringName)]
public class AiDbContext : AbpDbContext<AiDbContext>
{
    public DbSet<AiUsage> Usages { get; set; } = default!;
    // Prompt (audit table) and EvalSample arrive with 3.6/3.8.

    public AiDbContext(DbContextOptions<AiDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema(AiDbProperties.DbSchema);

        builder.ApplyConfigurationsFromAssembly(typeof(AiDbContext).Assembly);
    }
}
