using System.Xml.Serialization;
using System.Xml;

// One scripted spin authored as XML in StreamingAssets/DemoTickets/.
// PlayXml is the authoritative ticket override. Legacy ReelStops/Expected fields
// are kept so older demo files can still load while tickets are migrated.
[XmlRoot("Ticket")]
public class DemoTicket
{
    [XmlIgnore]
    public string Name;

    [XmlElement("Id")]
    public string Id;

    [XmlElement("Description")]
    public string Description;

    [XmlArray("ReelStops")]
    [XmlArrayItem("Stop")]
    public int[] ReelStops;

    [XmlArray("ExpectedWaysWins")]
    [XmlArrayItem("Win")]
    public ExpectedWin[] ExpectedWaysWins;

    [XmlElement("ExpectedTotalWin")]
    public int ExpectedTotalWin;

    [XmlAnyElement("PLAY")]
    public XmlElement PlayElement;

    [XmlIgnore]
    public string PlayXml;

    [XmlIgnore]
    public string DisplayName => !string.IsNullOrWhiteSpace(Name)
        ? Name
        : !string.IsNullOrWhiteSpace(Description)
            ? Description
            : Id;
}

public class ExpectedWin
{
    [XmlAttribute("symbol")]
    public int SymbolId;

    [XmlAttribute("columns")]
    public int ColumnsMatched;

    [XmlAttribute("ways")]
    public int Ways;

    [XmlAttribute("payout")]
    public int Payout;
}
