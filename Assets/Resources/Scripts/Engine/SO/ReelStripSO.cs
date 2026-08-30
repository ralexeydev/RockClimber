using UnityEngine;
using UnityEngine.Serialization;


//Editor to author reel strips. Drop symbol IDs into Symbols in any order;
// the distribution of IDs across the strip drives the slot's hit frequency.

[CreateAssetMenu(fileName = "ReelStrip", menuName = "RockClimber/Reel Strip")]

public class ReelStripSO : ScriptableObject
{
    [FormerlySerializedAs("Symbols")]
    public int[] SymbolsId;

    public ReelStrip ToReelStrip() => new ReelStrip(SymbolsId);
}
