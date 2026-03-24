using ExchangeRateMonitor.Options;
using ExchangeRateMonitor.Services;
using Microsoft.Extensions.Options;

namespace ExchangeRateMonitor.Workers;

public class ExchangeRateWorker : BackgroundService
{
    private readonly ICbrExchangeRateService _exchangeRateService;
    private readonly ICurrencyRepository _currencyRepository;
    private readonly ILogger<ExchangeRateWorker> _logger;
    private readonly TimeSpan _interval;

    public ExchangeRateWorker(
        ICbrExchangeRateService exchangeRateService,
        ICurrencyRepository currencyRepository,
        ILogger<ExchangeRateWorker> logger,
        IOptions<ExchangeRateWorkerOptions> options)
    {
        _exchangeRateService = exchangeRateService;
        _currencyRepository = currencyRepository;
        _logger = logger;
        _interval = TimeSpan.FromHours(options.Value.IntervalHours);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ExchangeRateWorker started. Interval: {Interval}", _interval);

        await FetchAndSaveRatesAsync(stoppingToken);

        using var timer = new PeriodicTimer(_interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await FetchAndSaveRatesAsync(stoppingToken);
        }
    }

    private async Task FetchAndSaveRatesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var rates = await _exchangeRateService.FetchRatesAsync(cancellationToken);
            await _currencyRepository.SaveRatesAsync(rates, cancellationToken);
            _logger.LogInformation("Exchange rates updated successfully");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Graceful shutdown
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update exchange rates");
        }
    }
}
