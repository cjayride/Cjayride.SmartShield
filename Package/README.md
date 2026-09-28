## SmartShield

Automatically equips a shield when you switch to a one-handed weapon. Works after bow/crossbow. No popups, no hotkeys.

Client-side. Drop `Cjayride.SmartShield.dll` in BepInEx `plugins`.

Disable AutoEquipShield if both are installed.

## Changelog

### 1.0.10

- Fix: first sword draw after spawn now auto-equips the shield (warmup no longer baselines the weapon without applying).

### 1.0.9

- Fix: hotbar deselect only unequips the shield when you put away a **one-handed weapon** — manually equipping a shield alone no longer gets yanked away.
- Fix: R sheath no longer unequips the shield in a HideHandItems prefix (that emptied both hands and made R pop weapons back out).
- Fix: drawing with R now triggers shield auto-equip reliably.

### 1.0.8

- `UnequipOnHotbarDeselect` default is now **true**.

### 1.0.7

- New: `UnequipOnHotbarDeselect` — unequip the shield when you deselect the weapon on the hotbar. `UnequipOnSheath` stays R-only.

### 1.0.6

- Fix: sheath retract now unequips the shield *before* vanilla stashes it on the back, then destroys the back mesh (`UpdateEquipmentVisuals`). Default `UnequipOnSheath = true`.
- Intentional: hotbar deselect keeps the shield unless you enable `UnequipOnHotbarDeselect`.
- Note: existing configs may still have `UnequipOnSheath = false` — set it to true (or delete the cfg line to pick up the new default).

### 1.0.5

- Fix: `UnequipOnSheath` now hooks vanilla `HideHandItems` (sheath) immediately and clears the back-slot shield (`SetLeftBackItem`). 1.0.4’s poll raced `Player.Update` and often missed.

### 1.0.4

- Fix: `UnequipOnSheath` was never applied. Sheathing (R) now retracts the shield when the option is enabled.

## Config Values

[Behavior]

Equip a shield whenever a one-handed weapon is equipped.
Setting type: Boolean
Default value: true
AutoEquipOnOneHanded = true

Also equip a shield after switching from a bow/crossbow (or other ranged) to a one-handed weapon.
Setting type: Boolean
Default value: true
EquipAfterRanged = true

Unequip the shield when a two-handed or ranged weapon is equipped.
Setting type: Boolean
Default value: true
UnequipOnTwoHanded = true

Unequip the shield when weapons are sheathed with R (clears the back slot).
Setting type: Boolean
Default value: true
UnequipOnSheath = true

Unequip the shield when you deselect a one-handed weapon on the hotbar (press the same slot again). Does not unequip a manually equipped shield alone.
Setting type: Boolean
Default value: true
UnequipOnHotbarDeselect = true

[Filters]

Comma-separated ItemType, skill, or prefab names. Empty = all one-handed weapons.
Setting type: String
Default value: 
AllowedWeaponTypes = 

Comma-separated shield prefab names that must never be auto-equipped.
Setting type: String
Default value: 
BlacklistShields = 

[General]

Master switch. No messages are ever shown.
Setting type: Boolean
Default value: true
Enabled = true

[ShieldSelection]

Best = highest block power. First = first shield in inventory. Preferred = use PreferredShield.
Setting type: String
Default value: Best
SelectionMode = Best

Prefab name used when SelectionMode is Preferred (example: ShieldWood).
Setting type: String
Default value: 
PreferredShield = 

## Contact

𝕏: x.com/cjayride

Discord: discord.gg/cjayride (find me at the top of the user list) "cjayride"

Twitch: twitch.tv/cjayride

## AI Generated

This code was AI Generated.