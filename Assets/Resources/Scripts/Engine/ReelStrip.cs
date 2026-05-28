//Reel strip: an ordered sequence of symbol IDs that acts like a physical
// reel. Spinning picks a random offset and reads N consecutive symbols, wrapping
// around the strip so a board with more copies of Low1 than High2 produces more
// Low1 outcomes naturally, without per-cell weighting.
public class ReelStrip
{
    private readonly int[] m_Symbols;

    public ReelStrip(int[] symbols)
    {
        m_Symbols = symbols;
    }

    public int Length => m_Symbols.Length;

    // Safe wrap-around for any index, including negative values.
    public int Get(int index)
    {
        int len = m_Symbols.Length;
        int normalizedIndex = index % len;

        if (normalizedIndex < 0)
            normalizedIndex += len;

        return m_Symbols[normalizedIndex];
    }
}
