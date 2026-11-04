# Changelog

## 1.0.14

- Throwing a spear no longer unequips the shield. The last spear leaving your hand is treated as a throw, not a hotbar deselect.

## 1.0.13

- Re-verified against Valheim 1.0.21 (Steam build 26249040): HideHandItems, ShowHandItems, EquipItem, UnequipItem, and VisEquipment left-back APIs are unchanged.
- Auto-equip now reacts to Equip/Unequip instead of polling 8 times per second. Update() is a slow safety net after spawn.
- Cached weapon/shield filter lists; scan inventory in place; skip shield-visual rebuild when nothing is attached.

## 1.0.12

- Verified against Valheim 1.0.21 (Steam build 26249040).
- BepInEx dependency set to `denikson-BepInExPack_Valheim-5.4.2360`.

## 1.0.11

- Default `UnequipOnSheath` is now **false** so R-sheath shows weapon + shield on your back (vanilla). Set `UnequipOnSheath = true` if you prefer weapon-only on the back.

## 1.0.10

- Fix: first sword draw after spawn now auto-equips the shield (warmup no longer baselines the weapon without applying).

## 1.0.9

- Fix: hotbar deselect only unequips the shield when you put away a **one-handed weapon** — manually equipping a shield alone no longer gets yanked away.
- Fix: R sheath no longer unequips the shield in a HideHandItems prefix (that emptied both hands and made R pop weapons back out).
- Fix: drawing with R now triggers shield auto-equip reliably.
