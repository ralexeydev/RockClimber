using System.Collections.Generic;

// Symbol catalog and payout values for a ways-pay slot. Trigger symbols never
// contribute to a ways win and never pay by themselves.
public class PayTable
{
    public class Entry
    {
        public int Id;
        public bool IsWild;
        public bool IsFreeplayTrigger;
        public bool IsFeatureTrigger;
        public int[] Payouts;

        public bool IsTrigger => IsFreeplayTrigger || IsFeatureTrigger;
    }

    public IReadOnlyList<Entry> Entries { get; }
    public IReadOnlyList<int> FreeSpinAwards { get; }
    public int FreeplayTriggerMinimumCount { get; }
    public int FeatureTriggerMinimumCount { get; }

    private readonly Dictionary<int, Entry> m_ById;

    public PayTable(
        Entry[] entries,
        int[] freeSpinAwards,
        int freeplayTriggerMinimumCount = 3,
        int featureTriggerMinimumCount = 3)
    {
        Entries = entries;
        FreeSpinAwards = freeSpinAwards;
        FreeplayTriggerMinimumCount = freeplayTriggerMinimumCount;
        FeatureTriggerMinimumCount = featureTriggerMinimumCount;

        m_ById = new Dictionary<int, Entry>(entries.Length);

        foreach (Entry entry in entries)
            m_ById[entry.Id] = entry;
    }

    public Entry Get(int id) => m_ById.TryGetValue(id, out Entry entry) ? entry : null;

    // Trigger symbols are never valid wild substitutes in a winline, even if an
    // old asset still has IsWild enabled during migration.
    public bool IsWild(int id) =>
        m_ById.TryGetValue(id, out Entry entry) && entry.IsWild && !entry.IsTrigger;

    public bool IsFreeplayTrigger(int id) =>
        m_ById.TryGetValue(id, out Entry entry) && entry.IsFreeplayTrigger;

    public bool IsFeatureTrigger(int id) =>
        m_ById.TryGetValue(id, out Entry entry) && entry.IsFeatureTrigger;

    public bool IsTrigger(int id) =>
        m_ById.TryGetValue(id, out Entry entry) && entry.IsTrigger;
}
