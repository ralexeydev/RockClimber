using System;
using System.Xml.Serialization;

[XmlRoot("WinPool")]
public class WinPool
{
    [XmlArray("Entries")]
    [XmlArrayItem("Win")]
    public WinPoolEntry[] Entries;

    // Weighted draw. Returns null if the pool is empty or all weights are <= 0.
    public WinPoolEntry Pick(Random rng)
    {
        if (Entries == null || Entries.Length == 0)
            return null;

        int total = 0;
        foreach (var e in Entries)
            if (e.Weight > 0)
                total += e.Weight;

        if (total <= 0)
            return null;

        int roll = rng.Next(0, total);
        foreach (var e in Entries)
        {
            if (e.Weight <= 0)
                continue;

            roll -= e.Weight;
            if (roll < 0)
                return e;
        }

        // Floating-point/rounding guard: return the last positive-weight entry.
        for (int i = Entries.Length - 1; i >= 0; i--)
            if (Entries[i].Weight > 0)
                return Entries[i];

        return null;
    }

    public bool TryValidate(int maximumBaseWinCents, out string error)
    {
        error = null;

        if (Entries == null || Entries.Length == 0)
        {
            error = "The win pool has no entries.";
            return false;
        }

        var seenAmounts = new System.Collections.Generic.HashSet<int>();

        foreach (WinPoolEntry entry in Entries)
        {
            if (entry.BaseWinCents < 0 || entry.BaseWinCents > maximumBaseWinCents)
            {
                error = $"Win-pool amount {entry.BaseWinCents} is outside the BaseGame range.";
                return false;
            }

            if (entry.Weight <= 0)
            {
                error = $"Win-pool amount {entry.BaseWinCents} has a non-positive weight.";
                return false;
            }

            if (!seenAmounts.Add(entry.BaseWinCents))
            {
                error = $"Win-pool amount {entry.BaseWinCents} is duplicated.";
                return false;
            }
        }

        return true;
    }
}

public class WinPoolEntry
{
    [XmlAttribute("baseWinCents")]
    public int BaseWinCents;

    [XmlAttribute("weight")]
    public int Weight;
}
