using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class BaseGamePlayModeTests
{
    [UnityTearDown]
    public IEnumerator RestoreTimeScale()
    {
        Time.timeScale = 1f;
        yield return null;
    }

    [UnityTest]
    public IEnumerator BaseGameCompletesPaidDemoAndErrorFlows()
    {
        SceneManager.LoadScene("RockClimber");
        yield return null;

        GameController game = UnityEngine.Object.FindObjectOfType<GameController>();
        Assert.That(game, Is.Not.Null);
        yield return WaitForState(game, GameState.Idle, 30f);
        Time.timeScale = 10f;

        Assert.That(game.SpinButtonView.IsInteractable, Is.True);
        Assert.That(game.BetController.CurrentBet, Is.EqualTo(25));

        int initialBalance = game.BalanceController.Balance;
        game.SpinButtonView.GetComponent<Button>().onClick.Invoke();
        yield return null;
        Assert.That(game.SpinButtonView.IsInteractable, Is.False);
        yield return WaitForState(game, GameState.Idle, 15f);

        int expectedBalance = initialBalance - game.BetController.CurrentBet + game.HudView.LastWinCents;
        Assert.That(game.BalanceController.Balance, Is.EqualTo(expectedBalance));

        Assert.That(game.DemoController.Tickets, Is.Not.Empty);
        foreach (DemoTicket demo in game.DemoController.Tickets)
        {
            int balanceBeforeDemo = game.BalanceController.Balance;
            game.DemoController.QueueTicket(demo);
            yield return WaitForState(game, GameState.Idle, 15f);
            Assert.That(game.BalanceController.Balance, Is.EqualTo(balanceBeforeDemo), demo.DisplayName);
        }

        game.WinPoolController.ForcedWin = true;
        game.WinPoolController.ForcedBaseWinCents = 1;
        int balanceBeforeError = game.BalanceController.Balance;
        LogAssert.Expect(LogType.Error, new Regex("RequestingResultState: Engine error"));
        LogAssert.Expect(LogType.Error, "ErrorState: Invalid spin result.");
        game.SpinButtonView.GetComponent<Button>().onClick.Invoke();
        yield return WaitForState(game, GameState.Error, 2f);
        Assert.That(game.SpinButtonView.IsInteractable, Is.False);
        yield return WaitForState(game, GameState.Idle, 5f);
        Assert.That(game.BalanceController.Balance, Is.EqualTo(balanceBeforeError));
    }

    private static IEnumerator WaitForState(GameController game, GameState expectedState, float timeoutSeconds)
    {
        float deadline = Time.realtimeSinceStartup + timeoutSeconds;

        while (game.CurrentState != expectedState && Time.realtimeSinceStartup < deadline)
            yield return null;

        Assert.That(game.CurrentState, Is.EqualTo(expectedState));
    }
}
