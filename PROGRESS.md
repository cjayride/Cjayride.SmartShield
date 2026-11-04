# SmartShield — keep shield on spear throw

## Goal
- When a player throws a spear, do not unequip the shield.

## Status
- Completed: 1.0.14 Release build succeeded.

## Output
- DLL: `D:\AI-Agent-Projects\cjaycraft\Cjayride.SmartShield\bin\Release\net481\Cjayride.SmartShield.dll`

## Changes
- `UnequipItem` prefix: if a throwable one-hander (Spears, or projectile attack) is unequipped while attacking, set a 1s ignore window so hotbar-deselect logic does not strip the shield.
- `Attack.OnAttackTrigger` also sets that window for one-handed projectile attacks.

## Commands
- `dotnet build Cjayride.SmartShield.csproj -c Release` — succeeded, 0 warnings.
