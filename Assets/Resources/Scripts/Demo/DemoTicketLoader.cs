using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

// Loads the ordered BUTTON list from StreamingAssets/DemoTickets/demos.xml.
public static class DemoTicketLoader
{
    public static List<DemoTicket> LoadAll(string path)
    {
        var tickets = new List<DemoTicket>();

        if (!File.Exists(path))
        {
            Debug.LogWarning($"DemoTicketLoader: file not found - {path}");
            return tickets;
        }

        try
        {
            XDocument document = XDocument.Load(path);
            XElement root = document.Root;
            if (root == null)
                throw new FormatException("Demo XML has no root element.");

            foreach (XElement button in root.Elements()
                         .Where(element => HasName(element, "BUTTON")))
            {
                tickets.Add(LoadButton(button));
            }

            if (tickets.Count == 0)
                Debug.LogWarning($"DemoTicketLoader: no BUTTON entries found in {path}");
        }
        catch (Exception exception)
        {
            Debug.LogError($"DemoTicketLoader: failed to parse {path} - {exception.Message}");
            tickets.Clear();
        }

        return tickets;
    }

    private static DemoTicket LoadButton(XElement button)
    {
        string name = ((string)button.Attribute("name"))?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new FormatException("Every BUTTON requires a non-empty 'name' attribute.");

        XElement play = button.Descendants()
            .FirstOrDefault(element => HasName(element, "PLAY"));

        // An escaped or CDATA PLAY payload is accepted too.
        if (play == null && !string.IsNullOrWhiteSpace(button.Value))
            play = XElement.Parse(button.Value.Trim());

        if (play == null || !HasName(play, "PLAY"))
            throw new FormatException($"BUTTON '{name}' does not contain a PLAY ticket.");

        string playXml = play.ToString(SaveOptions.DisableFormatting);
        PlayTicket.Parse(playXml);

        string id = ((string)button.Attribute("id"))?.Trim();
        return new DemoTicket
        {
            Name = name,
            Id = string.IsNullOrWhiteSpace(id) ? name : id,
            Description = name,
            PlayXml = playXml
        };
    }

    private static bool HasName(XElement element, string expected)
    {
        return string.Equals(element.Name.LocalName, expected, StringComparison.OrdinalIgnoreCase);
    }
}
