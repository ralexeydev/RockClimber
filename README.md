# RockClimber BaseGame

RockClimber is a Unity 2022.3 portfolio project that implements a deterministic, ways-pay slot BaseGame. The current milestone deliberately focuses on a coherent paid-spin loop before Freeplay and Bonus are added.

## Current milestone

- Five reels and three visible rows evaluated as ways wins.
- Eight bet levels from `$0.25` to `$2.00`.
- Weighted WinPool authored in base cents at bet level 1.
- Exact reel-stop catalog generated once during Boot.
- Normal paid outcomes capped at `5000 × BetLevel` and filtered to prevent feature activation.
- Reproducible XML demo tickets that never change the balance.
- Balance, bet, win, state and error HUD.
- EditMode and PlayMode test assemblies.

Freeplay, Bonus, Risk Game, audio and a distributable player are intentionally outside this milestone. The future Bonus contract will be ticket-driven: the engine will predetermine the result and the gem screen will reveal it reproducibly.

## Architecture

The project separates four responsibilities:

1. `Engine` contains board construction, ways evaluation, ticket parsing and exact outcome selection without scene lookups.
2. `States` owns the paid-spin lifecycle from Boot through settlement.
3. `WinPool` loads and validates weighted base-cent outcomes.
4. Views render reels, wins, controls and HUD state.

Boot validates the ScriptableObject configuration and visual references, builds all `13^5 = 371,293` reel-stop combinations in batches, then confirms that every configured WinPool amount has at least one compatible BaseGame board. Gameplay stays disabled if any contract is invalid.

## WinPool format

The runtime file is `Assets/StreamingAssets/WinPool/winpool.xml`:

```xml
<WinPool>
  <Entries>
    <Win baseWinCents="0" weight="1000" />
    <Win baseWinCents="250" weight="900" />
  </Entries>
</WinPool>
```

`baseWinCents` is the payout at bet level 1. The final win is `baseWinCents × BetLevel`. Weights are relative, must be positive and do not need to total 100.

## Running the project

1. Install Unity `2022.3.62f2` with Windows build support.
2. Open the repository root as a Unity project.
3. Open `Assets/Scenes/RockClimber.unity`.
4. Enter Play Mode and wait for the HUD to reach `IDLE`.
5. Use the Bet button to cycle levels, Spin for a paid outcome and Demo to replay authored tickets.

If Boot finds invalid reels, symbols, visuals, XML or unreachable pool values, the HUD displays the error and gameplay remains locked.

## Tests

Open **Window > General > Test Runner** and run both suites:

- EditMode validates configuration, all 22 WinPool amounts at all eight bet levels, exact ticket/result agreement, XML errors and transaction idempotency.
- PlayMode loads the production scene and exercises a paid spin, a balance-neutral demo and an invalid forced-win refund.

A generated C# project can also be checked with:

```powershell
dotnet build Assembly-CSharp.csproj --no-restore -v:minimal
```

The C# build is a static check only. Portfolio readiness requires passing EditMode tests, PlayMode tests and a manual visual check at 1920×1080 and 1280×720.

## Result contracts

- A paid spin must receive an exact base-win request from the validated WinPool.
- Board, ways, ticket, combination win, total win and settlement always derive from the same evaluation.
- Forced wins use the exact catalog and are development-only.
- Demo tickets control reel stops, but their declared amounts must match the evaluated board.
- A paid bet is debited, credited or refunded at most once.

## Portfolio and licensing

Source code in this repository is provided under the root MIT license. The included Rock Climber graphic kit is third-party material and is not automatically covered by that license. Do not publish or redistribute the art until its original license and attribution requirements are documented in `THIRD_PARTY_NOTICES.md`.

Portfolio screenshots belong under `Screenshots/` after the production scene passes Play Mode verification.
