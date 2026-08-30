using System;
using System.Xml.Serialization;

// A weighted pool of target win amounts authored as XML in StreamingAssets/WinPool/.
// Each entry's payout is a multiplier of the bet; the actual coin target is
// resolved as Multiplier * bet at draw time (see WinPoolController). Weights are
// relative and need not sum to any particular total.
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
}

public class WinPoolEntry
{
    // Payout as a multiple of the current bet. 0 = losing spin.
    [XmlAttribute("multiplier")]
    public int Multiplier;

    // Relative draw weight. Non-positive weights are ignored.
    [XmlAttribute("weight")]
    public int Weight;
}
