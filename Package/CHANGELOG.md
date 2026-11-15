# Changelog

## 1.0.18

- Cursor no longer flickers: Valheim mouse-capture is released while the menu is open (no per-OnGUI cursor fighting).
- Behavior checkboxes are larger (28px) and the whole row is clickable.
- Menu draw is limited to Repaint events so the overlay does not extra-fill during layout.

## 1.0.17

- Dropdown list is boxed with a gold border and spaced rows so it no longer blends into Preferred shield.
- Selected row and Apply use muted gold instead of a bright yellow fill.
- Shield icon on the title; behavior toggles each have a matching icon (sword+shield, axe, bow, two-hander, back, hotbar).

## 1.0.16

- Alt+S menu now locks mouse look and movement so you can click it.
- Larger SeneaL-styled window (dark plate, gold type) with the full config: enable, selection mode, preferred shield, behavior toggles, and filters.
- Right-click no longer closes the menu (Esc / Close / Alt+S).

## 1.0.15

- Alt+S opens a small SmartShield popup: choose Best or First shield in inventory, and toggle whether a 1H axe auto-equips a shield (off for tree chopping).

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
