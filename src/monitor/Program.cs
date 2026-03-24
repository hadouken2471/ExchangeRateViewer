using ExchangeRateMonitor.Options;
using ExchangeRateMonitor.Services;
using ExchangeRateMonitor.Workers;
using Polly;
using Polly.Extensions.Http;
using Serilog;

var builder = Host.CreateDefaultBuilder(args);

builder.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.ConfigureServices((context, services) =>
{
    services.Configure<ExchangeRateWorkerOptions>(
        context.Configuration.GetSection(ExchangeRateWorkerOptions.SectionName));

    services.AddHttpClient<ICbrExchangeRateService, CbrExchangeRateService>()
        .AddPolicyHandler(GetRetryPolicy());

    services.AddSingleton<ICurrencyRepository, CurrencyRepository>();
    services.AddHostedService<ExchangeRateWorker>();
});

await builder.Build().RunAsync();

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
            onRetry: (outcome, delay, attempt, _) =>
            {
                var reason = outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString();
                Console.WriteLine($"CBR request retry {attempt} after {delay.TotalSeconds}s. Reason: {reason}");
            });
}
