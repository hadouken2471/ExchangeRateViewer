namespace ExchangeRateMonitor.Options;

public class ExchangeRateWorkerOptions
{
    public const string SectionName = "ExchangeRateWorker";

    public int IntervalHours { get; init; } = 24;
    public string CbrApiUrl { get; init; } = "http://www.cbr.ru/scripts/XML_daily.asp";
}
