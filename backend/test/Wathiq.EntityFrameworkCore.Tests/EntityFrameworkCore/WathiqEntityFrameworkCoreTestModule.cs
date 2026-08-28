using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Uow;
using Wathiq.Documents.EntityFrameworkCore;

namespace Wathiq.EntityFrameworkCore;

[DependsOn(
    typeof(WathiqApplicationTestModule),
    typeof(WathiqEntityFrameworkCoreModule),
    typeof(WathiqDocumentsEntityFrameworkCoreModule),
    typeof(Wathiq.Reminders.EntityFrameworkCore.WathiqRemindersEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqliteModule)
)]
public class WathiqEntityFrameworkCoreTestModule : AbpModule
{
    private SqliteConnection? _sqliteConnection;

    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<AbpSqliteOptions>(x => x.BusyTimeout = null);
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<FeatureManagementOptions>(options =>
        {
            options.SaveStaticFeaturesToDatabase = false;
            options.IsDynamicFeatureStoreEnabled = false;
        });
        Configure<PermissionManagementOptions>(options =>
        {
            options.SaveStaticPermissionsToDatabase = false;
            options.IsDynamicPermissionStoreEnabled = false;
        });
        context.Services.AddAlwaysDisableUnitOfWorkTransaction();

        ConfigureInMemorySqlite(context.Services);

    }

    private void ConfigureInMemorySqlite(IServiceCollection services)
    {
        _sqliteConnection = CreateDatabaseAndGetConnection();

        services.Configure<AbpDbContextOptions>(options =>
        {
            options.Configure(context =>
            {
                context.DbContextOptions.UseSqlite(_sqliteConnection);
            });

            // The host EF module registers a per-context provider for DocumentsDbContext (own migrations
            // history). Per-context config replaces the global one, so the test host must override it too.
            options.Configure<DocumentsDbContext>(context =>
            {
                context.DbContextOptions.UseSqlite(_sqliteConnection);
            });
            options.Configure<Wathiq.Reminders.EntityFrameworkCore.RemindersDbContext>(context =>
            {
                context.DbContextOptions.UseSqlite(_sqliteConnection);
            });
        });
    }

    public override void OnApplicationShutdown(ApplicationShutdownContext context)
    {
        _sqliteConnection?.Dispose();
    }

    private static SqliteConnection CreateDatabaseAndGetConnection()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<WathiqDbContext>()
            .UseSqlite(connection)
            .Options;

        using (var context = new WathiqDbContext(options))
        {
            context.GetService<IRelationalDatabaseCreator>().CreateTables();
        }

        // Each module context creates its own tables on the same in-memory connection.
        var documentsOptions = new DbContextOptionsBuilder<DocumentsDbContext>().UseSqlite(connection).Options;
        using (var context = new DocumentsDbContext(documentsOptions))
        {
            context.GetService<IRelationalDatabaseCreator>().CreateTables();
        }

        var remindersOptions = new DbContextOptionsBuilder<Wathiq.Reminders.EntityFrameworkCore.RemindersDbContext>()
            .UseSqlite(connection).Options;
        using (var context = new Wathiq.Reminders.EntityFrameworkCore.RemindersDbContext(remindersOptions))
        {
            context.GetService<IRelationalDatabaseCreator>().CreateTables();
        }

        return connection;
    }
}
