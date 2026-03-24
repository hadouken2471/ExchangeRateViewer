using System.Globalization;
using System.Xml.Serialization;

namespace ExchangeRateMonitor.Models;

[XmlType("Valute")]
public class Valute
{
    [XmlAttribute("ID")]
    public string Id { get; set; } = string.Empty;

    [XmlElement("NumCode")]
    public string NumCode { get; set; } = string.Empty;

    [XmlElement("CharCode")]
    public string CharCode { get; set; } = string.Empty;

    [XmlElement("Nominal")]
    public int Nominal { get; set; }

    [XmlElement("Name")]
    public string Name { get; set; } = string.Empty;

    [XmlElement("Value")]
    public string Value { get; set; } = string.Empty;

    [XmlElement("VunitRate")]
    public string VunitRate { get; set; } = string.Empty;

    /// <summary>
    /// Парсит VunitRate из формата ЦБ РФ (запятая как разделитель, экспоненциальная нотация).
    /// </summary>
    public bool TryParseRate(out decimal rate)
    {
        return decimal.TryParse(
            VunitRate.Replace(',', '.'),
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out rate);
    }
}
