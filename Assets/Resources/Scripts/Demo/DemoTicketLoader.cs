using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Xml.Linq;
using UnityEngine;

// Loads every *.xml ticket under a directory into a list of DemoTicket.
// Sync File.ReadAllText is fine: small files, called once at boot, editor/standalone only.
public static class DemoTicketLoader
{
    public static List<DemoTicket> LoadAll(string directory)
    {
        var tickets = new List<DemoTicket>();

        if (!Directory.Exists(directory))
        {
            Debug.LogWarning($"DemoTicketLoader: directory not found — {directory}");
            return tickets;
        }

        foreach (var path in Directory.GetFiles(directory, "*.xml"))
        {
            try
            {
                var ticket = Load(path);
                if (ticket != null)
                    tickets.Add(ticket);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"DemoTicketLoader: failed to parse {path} — {e.Message}");
            }
        }

        tickets.Sort((a, b) => string.Compare(a.Id, b.Id, System.StringComparison.Ordinal));
        return tickets;
    }

    private static DemoTicket Load(string path)
    {
        XDocument doc = XDocument.Load(path);
        XElement root = doc.Root;

        if (root != null && root.Name.LocalName == "PLAY")
        {
            return new DemoTicket
            {
                Id = Path.GetFileNameWithoutExtension(path),
                Description = Path.GetFileNameWithoutExtension(path),
                PlayXml = root.ToString(SaveOptions.DisableFormatting)
            };
        }

        var serializer = new XmlSerializer(typeof(DemoTicket));
        using var stream = File.OpenRead(path);
        var ticket = (DemoTicket)serializer.Deserialize(stream);

        if (ticket?.PlayElement != null)
            ticket.PlayXml = ticket.PlayElement.OuterXml;

        if (ticket != null && string.IsNullOrWhiteSpace(ticket.PlayXml) && ticket.ReelStops != null)
        {
            int totalWin = ticket.ExpectedTotalWin;
            var legacy = new PlayTicket
            {
                ReelStops = ticket.ReelStops,
                CombinationWin = totalWin,
                TotalWin = totalWin,
                FeatureElementId = 0,
                ShowPrizes = false,
                BonusId = 0,
                FeatureId = 0
            };
            ticket.PlayXml = legacy.ToXml();
        }

        return ticket;
    }
}
