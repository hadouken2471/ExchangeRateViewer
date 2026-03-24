using ExchangeRateMonitor.Models;
using Npgsql;
using Polly;
using Polly.Retry;

namespace ExchangeRateMonitor.Services;

public class CurrencyRepository : ICurrencyRepository
{
    private readonly string _connectionString;
    private readonly ILogger<CurrencyRepository> _logger;

    private readonly AsyncRetryPolicy _retryPolicy;

    public CurrencyRepository(IConfiguration configuration, ILogger<CurrencyRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
                            ?? throw new InvalidOperationException("Строка подключения 'DefaultConnection' не найдена");
        _logger = logger;

        _retryPolicy = Policy
            .Handle<NpgsqlException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (exception, delay, attempt, _) =>
                {
                    _logger.LogWarning(exception,
                        "Попытка {Attempt} подключения к БД после {Delay} сек.",
                        attempt, delay.TotalSeconds);
                });
    }

    public async Task SaveRatesAsync(List<CurrencyRate> rates, CancellationToken cancellationToken)
    {
        await _retryPolicy.ExecuteAsync(async ct =>
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(ct);

            await using var transaction = await connection.BeginTransactionAsync(ct);

            await using var batch = new NpgsqlBatch(connection, transaction);

            foreach (var rate in rates)
            {
                var cmd = new NpgsqlBatchCommand(
                    "INSERT INTO finance.currency (id, name, rate) VALUES ($1, $2, $3) " +
                    "ON CONFLICT (id) DO UPDATE SET name = $2, rate = $3");
                cmd.Parameters.AddWithValue(rate.Id);
                cmd.Parameters.AddWithValue(rate.Name);
                cmd.Parameters.AddWithValue(rate.Rate);
                batch.BatchCommands.Add(cmd);
            }

            await batch.ExecuteNonQueryAsync(ct);
            await transaction.CommitAsync(ct);

            _logger.LogInformation("В базе упешно сохранено/обновлено {Count} валют", rates.Count);
        }, cancellationToken);
    }

}
