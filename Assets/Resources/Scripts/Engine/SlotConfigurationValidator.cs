using System.Collections.Generic;

public static class SlotConfigurationValidator
{
    private const int RequiredReels = 5;

    public static bool TryValidate(SlotConfigSO config, out string error)
    {
        error = null;

        if (config == null)
            return Fail("SlotConfig is not assigned.", out error);

        if (config.BaseReels == null || config.BaseReels.Strips == null ||
            config.BaseReels.Strips.Length != RequiredReels)
        {
            return Fail($"BaseGame requires exactly {RequiredReels} reel strips.", out error);
        }

        if (config.PayTable == null || config.PayTable.Entries == null || config.PayTable.Entries.Length == 0)
            return Fail("The paytable is empty or not assigned.", out error);

        var symbolIds = new HashSet<int>();
        int wildCount = 0;

        foreach (PayTableSO.Symbol symbol in config.PayTable.Entries)
        {
            if (symbol == null)
                return Fail("The paytable contains an empty symbol entry.", out error);

            if (!symbolIds.Add(symbol.Id))
                return Fail($"Paytable symbol ID {symbol.Id} is duplicated.", out error);

            if (symbol.IsWild)
                wildCount++;

            if (symbol.Payouts == null || symbol.Payouts.Length < 3)
                return Fail($"Paytable symbol ID {symbol.Id} requires payouts for 3, 4 and 5 reels.", out error);

            foreach (int payout in symbol.Payouts)
            {
                if (payout < 0)
                    return Fail($"Paytable symbol ID {symbol.Id} has a negative payout.", out error);
            }
        }

        if (wildCount != 1)
            return Fail("The BaseGame paytable requires exactly one wild symbol.", out error);

        for (int reel = 0; reel < config.BaseReels.Strips.Length; reel++)
        {
            ReelStripSO strip = config.BaseReels.Strips[reel];

            if (strip == null || strip.SymbolsId == null || strip.SymbolsId.Length == 0)
                return Fail($"BaseGame reel {reel + 1} is empty or not assigned.", out error);

            foreach (int symbolId in strip.SymbolsId)
            {
                if (!symbolIds.Contains(symbolId))
                    return Fail($"BaseGame reel {reel + 1} contains unknown symbol ID {symbolId}.", out error);
            }
        }

        if (config.PayTable.FreeplayTriggerMinimumCount < 1 || config.PayTable.FeatureTriggerMinimumCount < 1)
            return Fail("Trigger minimum counts must be positive.", out error);

        return true;
    }

    private static bool Fail(string message, out string error)
    {
        error = message;
        return false;
    }
}
