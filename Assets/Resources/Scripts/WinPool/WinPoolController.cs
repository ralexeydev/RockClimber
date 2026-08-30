using System.IO;
using UnityEngine;

// Owns the loaded win pool and, when active, draws a weighted target win for the
// next spin. RequestingResultState asks for a target (after the Demo-ticket check)
// and attaches it to the SpinRequest so the engine can search the reels for a
// matching board.
//
// Precedence is enforced by the caller: Demo ticket > WinPool target > normal RNG.
public class WinPoolController : BaseController
{
    [Header("Forced Win (Debug)")]
    [Tooltip("When enabled, non-demo spins use the forced total below instead of the win pool. This can remain enabled while the win pool is inactive.")]
    [SerializeField] private bool m_ForcedWin;

    [Tooltip("Requested total win in cents. For example, 100 pays $1.00.")]
    [SerializeField, Min(0)] private int m_ForcedTotalWin;

    [Header("Win Pool")]
    [Tooltip("When enabled, non-demo spins draw a weighted target win from the pool.")]
    [SerializeField] private bool m_Active;

    [Tooltip("File name under StreamingAssets/WinPool/ to load.")]
    [SerializeField] private string m_FileName = "winpool.xml";

    private WinPool m_Pool;
    private readonly System.Random m_Random = new();

    public bool Active
    {
        get => m_Active;
        set => m_Active = value;
    }

    public bool ForcedWin
    {
        get => m_ForcedWin;
        set => m_ForcedWin = value;
    }

    public int ForcedTotalWin
    {
        get => m_ForcedTotalWin;
        set => m_ForcedTotalWin = Mathf.Max(0, value);
    }

    // Last multiplier/target drawn, exposed for logging and UI.
    public int LastMultiplier { get; private set; }

    protected override void Initialize()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "WinPool", m_FileName);
        m_Pool = WinPoolLoader.Load(path);

        int count = m_Pool?.Entries?.Length ?? 0;
        Debug.Log($"WinPoolController: loaded {count} win-pool entries from {path}");
    }

    // Draws a target coin amount for the given bet, or null when the mode is
    // inactive or the pool is unusable. A return of 0 is a deliberate losing spin.
    public int? DrawTarget(int bet)
    {
        if (m_ForcedWin)
        {
            LastMultiplier = 0;
            return Mathf.Max(0, m_ForcedTotalWin);
        }

        if (!m_Active || m_Pool == null)
            return null;

        WinPoolEntry entry = m_Pool.Pick(m_Random);
        if (entry == null)
            return null;

        LastMultiplier = entry.Multiplier;
        return entry.Multiplier * bet;
    }
}
