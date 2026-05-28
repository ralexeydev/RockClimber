using System;
using UnityEngine;

// Editable paytable. Entries holds one row per symbol (low to high, wild, scatter)
// FreeSpinAwards maps scatter count to free-spin count:
// index 0 = 3 scatters, index 1 = 4, last = 5+ (clamped).

[CreateAssetMenu(fileName = "PayTable", menuName = "RockClimber/Pay Table")]
public class PayTableSO : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public int Id;
        public bool IsWild;
        public bool IsScatter;
        public int[] Payouts;
    }

    public Entry[] Entries;
    public int[] FreeSpinAwards = new int[3]; // default to 0 free spins for all scatter counts

    public PayTable ToPayTable()
    {
        var entries = new PayTable.Entry[Entries.Length];
        for (int i = 0; i < Entries.Length; i++)
        {
            entries[i] = new PayTable.Entry
            {
                Id = Entries[i].Id,
                IsWild = Entries[i].IsWild,
                IsScatter = Entries[i].IsScatter,
                Payouts = Entries[i].Payouts
            };
        }
        return new PayTable(entries, FreeSpinAwards);
    }
}
