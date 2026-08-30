//Reel strip: an ordered sequence of symbol IDs that acts like a physical
// reel. Spinning picks a random offset and reads N consecutive symbols, wrapping
// around the strip so a board with more copies of Low1 than High2 produces more
// Low1 outcomes naturally, without per-cell weighting.
public class ReelStrip
{
    private readonly int[] m_ReelStrips;

    public ReelStrip(int[] symbols)
    {
        if (symbols == null || symbols.Length == 0)
            throw new System.ArgumentException("ReelStrip requires at least one symbol; received an empty strip. Check that the ReelStripSO assets have their Symbols populated.");

        m_ReelStrips = symbols;
    }

    public int Length => m_ReelStrips.Length;

    // Safe wrap-around for any index, including negative values
    public int GetIndex(int index)
    {
        int len = m_ReelStrips.Length;
        int normalizedIndex = index % len;

        if (normalizedIndex < 0)
            normalizedIndex += len;

        return m_ReelStrips[normalizedIndex];
    }
}
