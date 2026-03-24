using ExchangeRateViewer.Finance.Infrastructure.Data;
using ExchangeRateViewer.Migrator.Workers;
using ExchangeRateViewer.Users.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = Host.CreateDefaultBuilder(args);

builder.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.ConfigureServices((context, services) =>
{
    var connectionString = context.Configuration.GetConnectionString("DefaultConnection");

    var migrationsAssembly = typeof(Program).Assembly.GetName().Name;

    services.AddDbContext<UserDbContext>(options =>
        options.UseNpgsql(connectionString, npgsql =>
        {
            npgsql.MigrationsHistoryTable("__EFMigrationsHistory", UserDbContext.Schema);
            npgsql.MigrationsAssembly(migrationsAssembly);
        }));

    services.AddDbContext<FinanceDbContext>(options =>
        options.UseNpgsql(connectionString, npgsql =>
        {
            npgsql.MigrationsHistoryTable("__EFMigrationsHistory", FinanceDbContext.Schema);
            npgsql.MigrationsAssembly(migrationsAssembly);
        }));

    services.AddHostedService<MigrationWorker>();
});

await builder.Build().RunAsync();
