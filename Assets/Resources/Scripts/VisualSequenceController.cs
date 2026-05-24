using System.Collections;
using UnityEngine;

// Placeholder visual controller. Each method is a stub coroutine with a timed wait.
// Replace the wait and log calls with real animation, reel-stop, and sound logic.
public class VisualSequenceController : BaseView
{
    [SerializeField] private float m_MockSpinDuration = 1.5f;
    [SerializeField] private float m_MockWinDuration = 1.0f;
    [SerializeField] private float m_MockFeatureDuration = 2.0f;

    public IEnumerator PlaySpinSequence(SpinResult spinResult)
    {
        Debug.Log("Playing spin visuals...");
        yield return new WaitForSeconds(m_MockSpinDuration);
        Debug.Log("Showing final board.");
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
}
