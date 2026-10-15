using System.IO;
using UnityEngine;
using UnityEngine.Serialization;

// Owns the loaded win pool and, when active, draws a weighted target win for the
// next spin. RequestingResultState asks for a target (after the Demo-ticket check)
// and attaches it to the SpinRequest so the engine can search the reels for a
// matching board.
//
// Precedence is enforced by the caller: Demo ticket > WinPool target > normal RNG.
public class WinPoolController : BaseController
{
    private const int MaximumBaseWinCents = 5000;

    [Header("Forced Win (Debug)")]
    [Tooltip("When enabled, non-demo spins use the forced total below instead of the win pool. This can remain enabled while the win pool is inactive.")]
    [SerializeField] private bool m_ForcedWin;

    [FormerlySerializedAs("m_ForcedTotalWin")]
    [Tooltip("Requested win in base cents at bet level 1.")]
    [SerializeField, Min(0)] private int m_ForcedBaseWinCents;

    [Header("Win Pool")]
    [Tooltip("When enabled, non-demo spins draw a weighted target win from the pool.")]
    [SerializeField] private bool m_Active;

    [Tooltip("File name under StreamingAssets/WinPool/ to load.")]
    [SerializeField] private string m_FileName = "winpool.xml";

    private WinPool m_Pool;
    private string m_LoadError;
    private bool m_LoadAttempted;
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

    public int ForcedBaseWinCents
    {
        get => m_ForcedBaseWinCents;
        set => m_ForcedBaseWinCents = Mathf.Max(0, value);
    }

    public int LastBaseWinCents { get; private set; }

    protected override void Initialize()
    {
        EnsureLoaded();
    }

    public void EnsureLoaded()
    {
        if (m_LoadAttempted)
            return;

        m_LoadAttempted = true;
        string path = Path.Combine(Application.streamingAssetsPath, "WinPool", m_FileName);
        WinPoolLoader.TryLoad(path, out m_Pool, out m_LoadError);

        int count = m_Pool?.Entries?.Length ?? 0;
        Debug.Log($"WinPoolController: loaded {count} win-pool entries from {path}");
    }

    public bool TryPrepare(ISlotEngine engine, out string error)
    {
        EnsureLoaded();
        error = m_LoadError;

        if (engine == null || !engine.IsReady)
        {
            error = "The exact BaseGame outcome catalog is not ready.";
            return false;
        }

        if (m_ForcedWin)
        {
            if (!engine.CanResolveBaseWin(m_ForcedBaseWinCents))
            {
                error = $"Forced base win {m_ForcedBaseWinCents} is not reachable.";
                return false;
            }

            return true;
        }

        if (!m_Active)
        {
            error = "The BaseGame win pool is disabled.";
            return false;
        }

        if (m_Pool == null)
            return false;

        if (!m_Pool.TryValidate(MaximumBaseWinCents, out error))
            return false;

        foreach (WinPoolEntry entry in m_Pool.Entries)
        {
            if (engine.CanResolveBaseWin(entry.BaseWinCents))
                continue;

            error = $"Win-pool amount {entry.BaseWinCents} has no exact BaseGame board.";
            return false;
        }

        return true;
    }

    public int? DrawBaseWinCents()
    {
        if (m_ForcedWin)
        {
            LastBaseWinCents = Mathf.Max(0, m_ForcedBaseWinCents);
            return LastBaseWinCents;
        }

        if (!m_Active || m_Pool == null)
            return null;

        WinPoolEntry entry = m_Pool.Pick(m_Random);
        if (entry == null)
            return null;

        LastBaseWinCents = entry.BaseWinCents;
        return entry.BaseWinCents;
    }
}
