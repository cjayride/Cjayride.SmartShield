# SmartShield — 1H axe auto-equip fix

## Goal
- With **Equip shield with a 1H axe** checked, auto-equip shield when drawing a one-handed axe.

## Status
- Completed: 1.0.20 Release build succeeded (0 warnings).

## Root cause (analysis)
- Axe opt-out used `EquipShieldWithAxe = false` by default (correct for chopping).
- When enabled, shield equip could still fail: immediate `Apply` on `EquipItem` could run before hands settled, and `Tick` would not re-run on the next frames because `_lastRight` already matched.
- `ShouldWantShield` now treats the axe toggle as an explicit enable path (not only via `AutoEquipOnOneHanded`).

## Files changed
- `src/ShieldService.cs` — `ShouldWantShield`, `ShouldStowShieldForRightHand`, stronger `IsOneHandedAxe`, `ApplyAfterEquipmentSettled`, deferred hook.
- `src/Plugin.cs` — coroutine `ApplyAfterEquipFrames`.
- `src/ModConfig.cs` — `SettingChanged` reapply for axe/butcher toggles.
- Version bump 1.0.20 (csproj, manifest, pack.ps1, CHANGELOG).

## Output
- DLL: `D:\AI-Agent-Projects\cjaycraft\Cjayride.SmartShield\bin\Release\net481\Cjayride.SmartShield.dll`

## Verify in-game
1. Alt+S: enable **Equip shield with a 1H axe** (and **Auto-equip** if using that master toggle).
2. Draw a 1H axe (Flint/Bronze/etc.) — shield should appear within ~1 frame.