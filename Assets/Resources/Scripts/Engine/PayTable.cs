using System.Collections.Generic;

// Symbol catalog + payout multipliers for a ways-pay slot.
// Payouts is a 3-element array: [0] = 3-of-a-kind, [1] = 4, [2] = 5.
// Final coin payout is computed by WaysEvaluator as ways * payout * bet.
//
// FreeSpinAwards maps scatter count to free-spin count: index 0 = 3 scatters,
// index 1 = 4, last index = 5+ (clamped).
public class PayTable
{
    public class Entry
    {
        public int Id;
        public bool IsWild;
        public bool IsScatter;
        public int[] Payouts;
    }

    public IReadOnlyList<Entry> Entries { get; }
    public IReadOnlyList<int> FreeSpinAwards { get; }

    private readonly Dictionary<int, Entry> m_ById;

    public PayTable(Entry[] entries, int[] freeSpinAwards)
    {
        Entries = entries;
        FreeSpinAwards = freeSpinAwards;

        m_ById = new Dictionary<int, Entry>(entries.Length);

        foreach (Entry e in entries)
            m_ById[e.Id] = e;
    }

    public Entry Get(int id) => m_ById.TryGetValue(id, out var e) ? e : null;
    public bool IsWild(int id) => m_ById.TryGetValue(id, out var e) && e.IsWild;
    public bool IsScatter(int id) => m_ById.TryGetValue(id, out var e) && e.IsScatter;
}
