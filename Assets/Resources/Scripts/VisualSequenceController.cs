using System.Collections;
using UnityEngine;

// Orchestrates visual sequences for spinning, wins, and features.
// Win and feature sequences are still placeholders pending visual implementation.
public class VisualSequenceController : BaseView
{
    // Assign the five ReelView components left to right in the Inspector.
    [SerializeField] private ReelView[] m_Reels;

    // All reels spin together for at least this long before any start stopping.
    [SerializeField] private float m_MinSpinDuration = 1.5f;

    // Time between each successive reel stopping, left to right.
    [SerializeField] private float m_ReelStopDelay = 0.2f;

    [SerializeField] private float m_MockWinDuration = 1.0f;
    [SerializeField] private float m_MockFeatureDuration = 2.0f;

    public IEnumerator PlaySpinSequence(SpinResult spinResult)
    {
        // Kick off all reels simultaneously.
        foreach (ReelView reel in m_Reels)
            reel.StartSpin();

        // Let every reel spin for the minimum duration before stopping begins.
        yield return new WaitForSeconds(m_MinSpinDuration);

        // Stop reels one by one left to right, staggered by m_ReelStopDelay.
        // The last reel's coroutine is stored so we can wait for it to fully finish.
        Coroutine lastReelStop = null;

        for (int reelIndex = 0; reelIndex < m_Reels.Length; reelIndex++)
        {
            int[] columnSymbols = ExtractColumnSymbols(spinResult.FinalBoard, reelIndex);
            lastReelStop = StartCoroutine(m_Reels[reelIndex].StopSpin(columnSymbols));

            if (reelIndex < m_Reels.Length - 1)
                yield return new WaitForSeconds(m_ReelStopDelay);
        }

        // Pause here until the final reel has revealed its symbols.
        yield return lastReelStop;
    }

    public IEnumerator PlayWinSequence(SpinResult spinResult)
    {
        Debug.Log($"Playing win visuals. Win: {spinResult.TotalWin}");
        yield return new WaitForSeconds(m_MockWinDuration);
    }

    public IEnumerator PlayFeatureSequence(SpinResult spinResult)
    {
        Debug.Log("Playing feature visuals...");
        yield return new WaitForSeconds(m_MockFeatureDuration);
    }

    // Extracts the three symbol IDs for a given reel from the 2D board array.
    private int[] ExtractColumnSymbols(int[,] board, int reelIndex)
    {
        int rowCount = board.GetLength(1);
        int[] symbols = new int[rowCount];

        for (int row = 0; row < rowCount; row++)
            symbols[row] = board[reelIndex, row];

        return symbols;
    }
}
