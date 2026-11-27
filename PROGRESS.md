# SmartShield — torch auto-equip option

## Goal
- Add **Equip shield with a torch** toggle (same pattern as 1H axe and butcher knife).
- User tests in Gale profile before Thunderstore ship.

## Status: build done — ready for Gale test

## Root cause
- Valheim torches are `ItemType.Torch`, not `OneHandedWeapon`; `ApplyInner` returned early.

## Changes
- `src/ModConfig.cs` — `EquipShieldWithTorch` (default `false`), menu reapply hook.
- `src/ShieldService.cs` — torch path in apply logic, `IsTorch`, `ShouldWantShield`, hotbar deselect.
- `src/OptionsMenu.cs` — Alt+S toggle.
- `src/MenuIcons.cs` — torch icon.
- Docs/manifest `1.0.21`.

## Verification
```text
dotnet build D:\AI-Agent-Projects\cjaycraft\Cjayride.SmartShield\Cjayride.SmartShield.csproj -c Release
```

## Gale test
1. Copy `bin\Release\net481\Cjayride.SmartShield.dll` over the profile plugin (back up first).
2. Alt+S → enable **Equip shield with a torch**.
3. Equip torch right hand → shield should auto-equip.
4. Disable toggle → shield stows when torch equipped.

## Next
- User confirms in Gale; then run `pack.ps1` to ship.