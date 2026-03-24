using ExchangeRateViewer.Finance.Infrastructure.Data;
using ExchangeRateViewer.Users.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ExchangeRateViewer.Migrator.Workers;

public class MigrationWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MigrationWorker> _logger;
    private readonly IHostApplicationLifetime _lifetime;

    public MigrationWorker(
        IServiceProvider serviceProvider,
        ILogger<MigrationWorker> logger,
        IHostApplicationLifetime lifetime)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _lifetime = lifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();

            await MigrateAsync<UserDbContext>(scope, stoppingToken);
            await MigrateAsync<FinanceDbContext>(scope, stoppingToken);

            _logger.LogInformation("Все миграции применены успешно");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Миграция завершилась с ошибкой");
            Environment.ExitCode = 1;
        }
        finally
        {
            _lifetime.StopApplication();
        }
    }

    private async Task MigrateAsync<TContext>(IServiceScope scope, CancellationToken cancellationToken)
        where TContext : DbContext
    {
        var contextName = typeof(TContext).Name;
        _logger.LogInformation("Применение миграций для {Context}...", contextName);

        var context = scope.ServiceProvider.GetRequiredService<TContext>();
        await context.Database.MigrateAsync(cancellationToken);

        _logger.LogInformation("Миграции для {Context} были успешно применены", contextName);
    }
}
