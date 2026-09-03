using System;
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
    typeof(Wathiq.Ai.EntityFrameworkCore.WathiqAiEntityFrameworkCoreModule),
    typeof(Wathiq.Guides.EntityFrameworkCore.WathiqGuidesEntityFrameworkCoreModule),
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
            options.Configure<Wathiq.Ai.EntityFrameworkCore.AiDbContext>(context =>
            {
                context.DbContextOptions.UseSqlite(_sqliteConnection);
            });
            options.Configure<Wathiq.Guides.EntityFrameworkCore.GuidesDbContext>(context =>
            {
                context.DbContextOptions.UseSqlite(_sqliteConnection);
            });
        });
    }

    public override void OnApplicationShutdown(ApplicationShutdownContext context)
    {
        try
        {
            _sqliteConnection?.Dispose();
        }
        catch (NullReferenceException)
        {
            // Microsoft.Data.Sqlite teardown race: Close() iterates its live-command list while
            // the finalizer thread clears entries of commands EF left undisposed. Assertions have
            // long passed by now and the in-memory DB dies with the process either way - a driver
            // race in teardown must not fail a green test (seen ~1 in 14 full-suite runs, 3.5).
        }
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

        var aiOptions = new DbContextOptionsBuilder<Wathiq.Ai.EntityFrameworkCore.AiDbContext>()
            .UseSqlite(connection).Options;
        using (var context = new Wathiq.Ai.EntityFrameworkCore.AiDbContext(aiOptions))
        {
            context.GetService<IRelationalDatabaseCreator>().CreateTables();
        }

        // Guides has no tables yet (empty Initial, 5.1) - CreateTables is a no-op but keeps the
        // ritual: forget this block in 5.2 and every guides.* test dies with "no such table".
        var guidesOptions = new DbContextOptionsBuilder<Wathiq.Guides.EntityFrameworkCore.GuidesDbContext>()
            .UseSqlite(connection).Options;
        using (var context = new Wathiq.Guides.EntityFrameworkCore.GuidesDbContext(guidesOptions))
        {
            context.GetService<IRelationalDatabaseCreator>().CreateTables();
        }

        return connection;
    }
}
