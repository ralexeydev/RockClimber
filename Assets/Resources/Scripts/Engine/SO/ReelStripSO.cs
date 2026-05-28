using UnityEngine;


//Editor to author reel strips. Drop symbol IDs into Symbols in any order;
// the distribution of IDs across the strip drives the slot's hit frequency.    

[CreateAssetMenu(fileName = "ReelStrip", menuName = "RockClimber/Reel Strip")]

public class ReelStripSO : ScriptableObject
{
    public int[] Symbols;

    public ReelStrip ToReelStrip() => new ReelStrip(Symbols);
}
