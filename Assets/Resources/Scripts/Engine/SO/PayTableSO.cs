using System;
using UnityEngine;

// Editable paytable. Freeplay and feature triggers are evaluated separately;
// they never pay and never participate in ways wins.
[CreateAssetMenu(fileName = "PayTable", menuName = "RockClimber/Pay Table")]
public class PayTableSO : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public int Id;
        public bool IsWild;
        public bool IsFreeplayTrigger;
        public bool IsFeatureTrigger;
        public int[] Payouts;
    }

    public Entry[] Entries;
    public int[] FreeSpinAwards = new int[3];
    [Min(1)] public int FreeplayTriggerMinimumCount = 3;
    [Min(1)] public int FeatureTriggerMinimumCount = 3;

    public PayTable ToPayTable()
    {
        PayTable.Entry[] entries = new PayTable.Entry[Entries.Length];
        for (int i = 0; i < Entries.Length; i++)
        {
            entries[i] = new PayTable.Entry
            {
                Id = Entries[i].Id,
                IsWild = Entries[i].IsWild,
                IsFreeplayTrigger = Entries[i].IsFreeplayTrigger,
                IsFeatureTrigger = Entries[i].IsFeatureTrigger,
                Payouts = Entries[i].Payouts
            };
        }

        return new PayTable(
            entries,
            FreeSpinAwards,
            FreeplayTriggerMinimumCount,
            FeatureTriggerMinimumCount);
    }
}
