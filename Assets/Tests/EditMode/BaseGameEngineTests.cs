using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BaseGameEngineTests
{
    private SlotEngineController m_Engine;
    private WinPool m_Pool;

    [OneTimeSetUp]
    public void BuildCatalog()
    {
        SlotConfigSO config = Resources.Load<SlotConfigSO>("Scripts/Engine/SO/SlotConfig");
        Assert.That(config, Is.Not.Null);
        Assert.That(SlotConfigurationValidator.TryValidate(config, out string error), Is.True, error);

        m_Engine = new SlotEngineController(config);
        m_Engine.BeginInitialization();

        while (!m_Engine.StepInitialization(5000))
        {
        }

        string path = Path.Combine(Application.streamingAssetsPath, "WinPool", "winpool.xml");
        Assert.That(WinPoolLoader.TryLoad(path, out m_Pool, out error), Is.True, error);
        Assert.That(m_Pool.TryValidate(5000, out error), Is.True, error);
    }

    [Test]
    public void WinPoolContainsTwentyTwoUniqueReachableAmounts()
    {
        Assert.That(m_Pool.Entries, Has.Length.EqualTo(22));

        foreach (WinPoolEntry entry in m_Pool.Entries)
            Assert.That(m_Engine.CanResolveBaseWin(entry.BaseWinCents), Is.True, entry.BaseWinCents.ToString());
    }

    [Test]
    public void EveryPoolAmountPaysExactlyAtEveryBetLevel()
    {
        foreach (WinPoolEntry entry in m_Pool.Entries)
        {
            for (int betLevel = BetController.MinBetLevel; betLevel <= BetController.MaxBetLevel; betLevel++)
            {
                SpinResult result = m_Engine.GenerateSpinResult(new SpinRequest
                {
                    Bet = betLevel * BetController.BetStepCents,
                    BetLevel = betLevel,
                    Mode = GameMode.BaseGame,
                    RequestedBaseWinCents = entry.BaseWinCents
                });

                int expectedWin = entry.BaseWinCents * betLevel;
                Assert.That(result.IsValid, Is.True);
                Assert.That(result.TotalWin, Is.EqualTo(expectedWin));
                Assert.That(result.CombinationWin, Is.EqualTo(expectedWin));
                Assert.That(result.WaysWins.Sum(win => win.Payout), Is.EqualTo(expectedWin));
                Assert.That(result.HasFreeplay, Is.False);
                Assert.That(result.HasFeature, Is.False);

                PlayTicket ticket = PlayTicket.Parse(result.PlayTicketXml);
                Assert.That(ticket.TotalWin, Is.EqualTo(expectedWin));
                Assert.That(ticket.CombinationWin, Is.EqualTo(expectedWin));
            }
        }
    }

    [Test]
    public void DemoTicketPayoutMismatchIsRejected()
    {
        const string invalidTicket =
            "<PLAY RS=\"0 0 3 0 0\" sWin=\"1\" TotalWin=\"1\"><FEATURE id=\"0\"/><FID id=\"0\"/></PLAY>";

        var request = new SpinRequest
        {
            Bet = BetController.BetStepCents,
            BetLevel = BetController.MinBetLevel,
            Mode = GameMode.BaseGame,
            PlayTicketOverrideXml = invalidTicket
        };

        Assert.Throws<FormatException>(() => m_Engine.GenerateSpinResult(request));
    }

    [Test]
    public void AllSixDemoTicketsAreExactAndReproducible()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "DemoTickets", "demos.xml");
        var tickets = DemoTicketLoader.LoadAll(path);
        Assert.That(tickets, Has.Count.EqualTo(6));

        foreach (DemoTicket demo in tickets)
        {
            var request = new SpinRequest
            {
                Bet = BetController.BetStepCents,
                BetLevel = BetController.MinBetLevel,
                Mode = GameMode.BaseGame,
                PlayTicketOverrideXml = demo.PlayXml
            };

            SpinResult first = m_Engine.GenerateSpinResult(request);
            SpinResult second = m_Engine.GenerateSpinResult(request);
            PlayTicket ticket = PlayTicket.Parse(demo.PlayXml);

            Assert.That(first.IsValid, Is.True, demo.DisplayName);
            Assert.That(first.TotalWin, Is.EqualTo(ticket.TotalWin), demo.DisplayName);
            Assert.That(first.CombinationWin, Is.EqualTo(ticket.CombinationWin), demo.DisplayName);
            Assert.That(first.PlayTicketXml, Is.EqualTo(second.PlayTicketXml), demo.DisplayName);
            AssertBoardsEqual(first.FinalBoard, second.FinalBoard, demo.DisplayName);
        }
    }

    [Test]
    public void WaysEvaluationCoversLossWildsMultipleWaysAndTriggers()
    {
        PayTable payTable = CreateTestPayTable();
        WaysEvaluator.Result loss = WaysEvaluator.Evaluate(CreateBoard(
            1, 5, 1, 5, 1,
            5, 5, 5, 5, 5,
            5, 5, 5, 5, 5), payTable, 1);
        Assert.That(loss.TotalWin, Is.Zero);

        WaysEvaluator.Result pureWild = WaysEvaluator.Evaluate(CreateBoard(
            2, 2, 2, 5, 5,
            5, 5, 5, 5, 5,
            5, 5, 5, 5, 5), payTable, 1);
        Assert.That(pureWild.TotalWin, Is.EqualTo(20));
        Assert.That(pureWild.WaysWins, Has.Count.EqualTo(1));
        Assert.That(pureWild.WaysWins[0].SymbolId, Is.EqualTo(2));

        WaysEvaluator.Result substituted = WaysEvaluator.Evaluate(CreateBoard(
            1, 2, 1, 5, 5,
            5, 5, 5, 5, 5,
            5, 5, 5, 5, 5), payTable, 1);
        Assert.That(substituted.TotalWin, Is.EqualTo(10));
        Assert.That(substituted.WaysWins[0].SymbolId, Is.EqualTo(1));

        WaysEvaluator.Result multipleWays = WaysEvaluator.Evaluate(CreateBoard(
            1, 1, 1, 5, 5,
            1, 1, 1, 5, 5,
            5, 5, 5, 5, 5), payTable, 1);
        Assert.That(multipleWays.TotalWin, Is.EqualTo(80));
        Assert.That(multipleWays.WaysWins[0].Ways, Is.EqualTo(8));

        WaysEvaluator.Result isolatedTriggers = WaysEvaluator.Evaluate(CreateBoard(
            3, 4, 5, 5, 5,
            5, 5, 5, 5, 5,
            5, 5, 5, 5, 5), payTable, 1);
        Assert.That(isolatedTriggers.FreeplayTriggerCount, Is.EqualTo(1));
        Assert.That(isolatedTriggers.FeatureTriggerCount, Is.EqualTo(1));
        Assert.That(isolatedTriggers.FreeSpinsAwarded, Is.Zero);
        Assert.That(isolatedTriggers.FeatureTriggered, Is.False);

        WaysEvaluator.Result activatedTriggers = WaysEvaluator.Evaluate(CreateBoard(
            3, 3, 3, 4, 4,
            5, 5, 5, 4, 5,
            5, 5, 5, 5, 5), payTable, 1);
        Assert.That(activatedTriggers.FreeSpinsAwarded, Is.GreaterThan(0));
        Assert.That(activatedTriggers.FeatureTriggered, Is.True);
    }

    [Test]
    public void ExactCatalogAllowsIsolatedTriggersAndRejectsFeatureActivations()
    {
        PayTable payTable = CreateTestPayTable();
        ReelStrip[] isolatedStrips =
        {
            new(new[] { 3, 5, 5 }),
            new(new[] { 4, 5, 5 }),
            new(new[] { 5, 5, 5 }),
            new(new[] { 5, 5, 5 }),
            new(new[] { 5, 5, 5 })
        };
        var isolatedCatalog = new ExactOutcomeCatalog(isolatedStrips, payTable, 5, 3, 5000);
        isolatedCatalog.BuildAll();
        Assert.That(isolatedCatalog.ContainsBaseWin(0), Is.True);

        ReelStrip[] activatingStrips =
        {
            new(new[] { 3 }),
            new(new[] { 5 }),
            new(new[] { 5 }),
            new(new[] { 5 }),
            new(new[] { 5 })
        };
        var activatingCatalog = new ExactOutcomeCatalog(activatingStrips, payTable, 5, 3, 5000);
        activatingCatalog.BuildAll();
        Assert.That(activatingCatalog.ReachableBaseWins, Is.Empty);
    }

    [Test]
    public void InvalidWinPoolDataIsRejected()
    {
        var nonPositiveWeight = new WinPool
        {
            Entries = new[] { new WinPoolEntry { BaseWinCents = 0, Weight = 0 } }
        };
        Assert.That(nonPositiveWeight.TryValidate(5000, out string error), Is.False);
        Assert.That(error, Does.Contain("non-positive weight"));

        var duplicateAmount = new WinPool
        {
            Entries = new[]
            {
                new WinPoolEntry { BaseWinCents = 10, Weight = 1 },
                new WinPoolEntry { BaseWinCents = 10, Weight = 1 }
            }
        };
        Assert.That(duplicateAmount.TryValidate(5000, out error), Is.False);
        Assert.That(error, Does.Contain("duplicated"));

        var outOfRangeAmount = new WinPool
        {
            Entries = new[] { new WinPoolEntry { BaseWinCents = 5001, Weight = 1 } }
        };
        Assert.That(outOfRangeAmount.TryValidate(5000, out error), Is.False);
        Assert.That(error, Does.Contain("outside"));
        Assert.That(m_Engine.CanResolveBaseWin(1), Is.False);
    }

    [Test]
    public void MalformedWinPoolXmlReturnsAnActionableError()
    {
        string path = Path.Combine(Application.temporaryCachePath, $"invalid-winpool-{Guid.NewGuid():N}.xml");
        File.WriteAllText(path, "<WinPool><Entries>");
        LogAssert.Expect(LogType.Error, new Regex("Failed to parse win pool"));

        try
        {
            Assert.That(WinPoolLoader.TryLoad(path, out WinPool pool, out string error), Is.False);
            Assert.That(pool, Is.Null);
            Assert.That(error, Does.Contain("Failed to parse"));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Test]
    public void ConfigurationValidationRejectsDuplicateUnknownAndIncompleteData()
    {
        SlotConfigSO config = CreateValidTestConfig(out ReelStripSetSO reels, out PayTableSO payTable);

        try
        {
            payTable.Entries[1].Id = payTable.Entries[0].Id;
            Assert.That(SlotConfigurationValidator.TryValidate(config, out string error), Is.False);
            Assert.That(error, Does.Contain("duplicated"));

            payTable.Entries[1].Id = 2;
            reels.Strips[0].SymbolsId[0] = 99;
            Assert.That(SlotConfigurationValidator.TryValidate(config, out error), Is.False);
            Assert.That(error, Does.Contain("unknown symbol"));

            reels.Strips[0].SymbolsId[0] = 1;
            ReelStripSO[] completeStrips = reels.Strips;
            reels.Strips = completeStrips.Take(4).ToArray();
            Assert.That(SlotConfigurationValidator.TryValidate(config, out error), Is.False);
            Assert.That(error, Does.Contain("exactly 5"));

            reels.Strips = completeStrips;
            payTable.Entries[0].Payouts = new[] { 1, 2 };
            Assert.That(SlotConfigurationValidator.TryValidate(config, out error), Is.False);
            Assert.That(error, Does.Contain("3, 4 and 5"));
        }
        finally
        {
            DestroyTestConfig(config, reels, payTable);
        }
    }

    [Test]
    public void MissingWinPoolFileReturnsAnActionableError()
    {
        string missingPath = Path.Combine(Application.temporaryCachePath, "missing-winpool.xml");
        UnityEngine.TestTools.LogAssert.Expect(LogType.Error, $"Win-pool file not found: {missingPath}");
        bool loaded = WinPoolLoader.TryLoad(missingPath, out WinPool pool, out string error);

        Assert.That(loaded, Is.False);
        Assert.That(pool, Is.Null);
        Assert.That(error, Does.Contain("not found"));
    }

    [Test]
    public void SpinTransactionCannotCreditOrRefundTwice()
    {
        GameObject services = new("TransactionServices");
        BalanceController balance = services.AddComponent<BalanceController>();
        BetController bet = services.AddComponent<BetController>();
        var context = new GameContext(null, null, balance, bet, null, null, null, null, null, null, null, null, null);
        context.CurrentBet = BetController.BetStepCents;

        Assert.That(context.TryDebitBet(), Is.True);
        context.CreditWinOnce(100);
        context.CreditWinOnce(100);
        context.RefundBetOnce();
        Assert.That(balance.Balance, Is.EqualTo(10075));

        UnityEngine.Object.DestroyImmediate(services);
    }

    private static PayTable CreateTestPayTable()
    {
        return new PayTable(new[]
        {
            new PayTable.Entry { Id = 1, Payouts = new[] { 10, 20, 30 } },
            new PayTable.Entry { Id = 2, IsWild = true, Payouts = new[] { 20, 40, 60 } },
            new PayTable.Entry { Id = 3, IsFreeplayTrigger = true, Payouts = new[] { 0, 0, 0 } },
            new PayTable.Entry { Id = 4, IsFeatureTrigger = true, Payouts = new[] { 0, 0, 0 } },
            new PayTable.Entry { Id = 5, Payouts = new[] { 0, 0, 0 } }
        }, new[] { 5, 8, 10 }, 3, 3);
    }

    private static int[,] CreateBoard(params int[] symbols)
    {
        Assert.That(symbols, Has.Length.EqualTo(15));
        int[,] board = new int[5, 3];

        for (int row = 0; row < 3; row++)
        {
            for (int reel = 0; reel < 5; reel++)
                board[reel, row] = symbols[row * 5 + reel];
        }

        return board;
    }

    private static SlotConfigSO CreateValidTestConfig(out ReelStripSetSO reels, out PayTableSO payTable)
    {
        payTable = ScriptableObject.CreateInstance<PayTableSO>();
        payTable.Entries = new[]
        {
            new PayTableSO.Symbol { Id = 1, Payouts = new[] { 1, 2, 3 } },
            new PayTableSO.Symbol { Id = 2, IsWild = true, Payouts = new[] { 2, 4, 6 } }
        };
        payTable.FreeSpinAwards = new[] { 5, 8, 10 };
        payTable.FreeplayTriggerMinimumCount = 3;
        payTable.FeatureTriggerMinimumCount = 3;

        reels = ScriptableObject.CreateInstance<ReelStripSetSO>();
        reels.Strips = new ReelStripSO[5];
        for (int reel = 0; reel < reels.Strips.Length; reel++)
        {
            reels.Strips[reel] = ScriptableObject.CreateInstance<ReelStripSO>();
            reels.Strips[reel].SymbolsId = new[] { 1, 2 };
        }

        SlotConfigSO config = ScriptableObject.CreateInstance<SlotConfigSO>();
        config.BaseReels = reels;
        config.PayTable = payTable;
        return config;
    }

    private static void DestroyTestConfig(SlotConfigSO config, ReelStripSetSO reels, PayTableSO payTable)
    {
        foreach (ReelStripSO strip in reels.Strips)
            UnityEngine.Object.DestroyImmediate(strip);

        UnityEngine.Object.DestroyImmediate(config);
        UnityEngine.Object.DestroyImmediate(reels);
        UnityEngine.Object.DestroyImmediate(payTable);
    }

    private static void AssertBoardsEqual(int[,] expected, int[,] actual, string message)
    {
        Assert.That(actual.GetLength(0), Is.EqualTo(expected.GetLength(0)), message);
        Assert.That(actual.GetLength(1), Is.EqualTo(expected.GetLength(1)), message);

        for (int reel = 0; reel < expected.GetLength(0); reel++)
        {
            for (int row = 0; row < expected.GetLength(1); row++)
                Assert.That(actual[reel, row], Is.EqualTo(expected[reel, row]), message);
        }
    }
}
