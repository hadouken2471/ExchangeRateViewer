using System.Xml.Serialization;

namespace ExchangeRateMonitor.Models;

[XmlRoot("ValCurs")]
public class ValCurs
{
    [XmlAttribute("Date")]
    public string Date { get; set; } = string.Empty;

    [XmlAttribute("name")]
    public string Name { get; set; } = string.Empty;

    [XmlElement("Valute")]
    public List<Valute> Valutes { get; set; } = new();
}
