using UnityEngine;

// Root config for a slot game mode (base game, free spins, etc)

[CreateAssetMenu(fileName = "SlotConfig", menuName = "RockClimber/Slot Config")]
public class SlotConfigSO : ScriptableObject
{
    public ReelStripSetSO BaseReels;
    public ReelStripSetSO FreeSpinReels;
    public PayTableSO PayTable;
}
