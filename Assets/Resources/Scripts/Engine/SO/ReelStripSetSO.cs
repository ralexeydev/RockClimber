using UnityEngine;

// A complete set of reel strips for one game mode (5 strips, one per reel column).
[CreateAssetMenu(fileName = "ReelStripSet", menuName = "RockClimber/Reel Strip Set")]
public class ReelStripSetSO : ScriptableObject
{
    public ReelStripSO[] Strips;

    public ReelStrip[] ToReelStripArray()
    {
        var result = new ReelStrip[Strips.Length];
        for (int i = 0; i < Strips.Length; i++)
            result[i] = Strips[i].ToReelStrip();
        return result;
    }
}
