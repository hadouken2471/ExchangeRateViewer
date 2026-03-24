using System.Text;
using System.Xml.Serialization;
using ExchangeRateMonitor.Models;
using ExchangeRateMonitor.Options;
using Microsoft.Extensions.Options;

namespace ExchangeRateMonitor.Services;

public class CbrExchangeRateService : ICbrExchangeRateService
{
    private static readonly XmlSerializer Serializer = new(typeof(ValCurs));

    private readonly HttpClient _httpClient;
    private readonly ILogger<CbrExchangeRateService> _logger;
    private readonly string _cbrUrl;

    public CbrExchangeRateService(HttpClient httpClient, ILogger<CbrExchangeRateService> logger, IOptions<ExchangeRateWorkerOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _cbrUrl = options.Value.CbrApiUrl;
    }

    public async Task<List<CurrencyRate>> FetchRatesAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Получение актуальных курсов валют ЦБ РФ...");

        var response = await _httpClient.GetByteArrayAsync(_cbrUrl, cancellationToken);

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var xml = Encoding.GetEncoding("windows-1251").GetString(response);

        using var reader = new StringReader(xml);
        var valCurs = (ValCurs?)Serializer.Deserialize(reader)
                      ?? throw new InvalidOperationException("Ошибка при десериализации ответа ЦБ РФ");

        _logger.LogInformation("Получено {Count} валют на дату {Date}", valCurs.Valutes.Count, valCurs.Date);

        var rates = new List<CurrencyRate>(valCurs.Valutes.Count);

        foreach (var valute in valCurs.Valutes)
        {
            if (!valute.TryParseRate(out var rate))
            {
                _logger.LogWarning("Ошибка парсинга курса для {CharCode}: '{VunitRate}'", valute.CharCode, valute.VunitRate);
                continue;
            }

            rates.Add(new CurrencyRate
            {
                Id = valute.CharCode,
                Name = valute.Name,
                Rate = rate
            });
        }

        return rates;
    }
}
