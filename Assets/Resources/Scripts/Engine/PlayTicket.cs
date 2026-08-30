using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

public class PlayTicket
{
    public int[] ReelStops;
    public int CombinationWin;
    public int TotalWin;
    public int FeatureElementId;
    public int FeatureId;

    public static PlayTicket FromResult(int[] reelStops, WaysEvaluator.Result eval)
    {
        int totalWin = eval?.TotalWin ?? 0;
        bool hasFeature = eval?.FeatureTriggered ?? false;

        return new PlayTicket
        {
            ReelStops = reelStops,
            CombinationWin = totalWin,
            TotalWin = totalWin,
            FeatureElementId = hasFeature ? 1 : 0,
            FeatureId = hasFeature ? 1 : 0
        };
    }

    public static PlayTicket Parse(string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
            throw new ArgumentException("Play ticket XML is empty.", nameof(xml));

        XElement play = XElement.Parse(xml);
        if (!string.Equals(play.Name.LocalName, "PLAY", StringComparison.OrdinalIgnoreCase))
            throw new FormatException("Play ticket root must be PLAY.");

        return new PlayTicket
        {
            ReelStops = ParseReelStops(ReadRequired(play, "RS")),
            CombinationWin = ReadInt(play, "sWin", 0),
            TotalWin = ReadInt(play, "TotalWin", 0),
            FeatureElementId = ReadChildId(play, "FEATURE"),
            FeatureId = ReadChildId(play, "FID")
        };
    }

    public string ToXml()
    {
        string reelStops = string.Join(" ", ReelStops ?? Array.Empty<int>());

        XElement play = new(
            "PLAY",
            new XAttribute("RS", reelStops),
            new XAttribute("sWin", CombinationWin.ToString(CultureInfo.InvariantCulture)),
            new XAttribute("TotalWin", TotalWin.ToString(CultureInfo.InvariantCulture)),
            new XElement(
                "FEATURE",
                new XAttribute("id", FeatureElementId.ToString(CultureInfo.InvariantCulture))),
            new XElement("FID", new XAttribute("id", FeatureId.ToString(CultureInfo.InvariantCulture))));

        return play.ToString(SaveOptions.DisableFormatting);
    }

    private static string ReadRequired(XElement element, string attributeName)
    {
        XAttribute value = element.Attribute(attributeName);
        if (value == null)
            throw new FormatException($"PLAY is missing required '{attributeName}' attribute.");
        return value.Value;
    }

    private static int ReadInt(XElement element, string attributeName, int fallback)
    {
        XAttribute value = element.Attribute(attributeName);
        return value != null &&
               int.TryParse(value.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed)
            ? parsed
            : fallback;
    }

    private static int ReadChildId(XElement parent, string childName)
    {
        XElement child = parent.Elements().FirstOrDefault(element =>
            string.Equals(element.Name.LocalName, childName, StringComparison.OrdinalIgnoreCase));

        if (child == null)
            return 0;

        XAttribute id = child.Attribute("id");
        return id != null &&
               int.TryParse(id.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed)
            ? parsed
            : 0;
    }

    private static int[] ParseReelStops(string value)
    {
        return value
            .Split(new[] { ' ', ',', ';', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(stop => int.Parse(stop, CultureInfo.InvariantCulture))
            .ToArray();
    }
}
