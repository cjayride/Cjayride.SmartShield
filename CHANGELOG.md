# Changelog

## 1.0.11

- Default `UnequipOnSheath` is now **false** so R-sheath shows weapon + shield on your back (vanilla). Set `UnequipOnSheath = true` if you prefer weapon-only on the back.

## 1.0.10

- Fix: first sword draw after spawn now auto-equips the shield (warmup no longer baselines the weapon without applying).

## 1.0.9

- Fix: hotbar deselect only unequips the shield when you put away a **one-handed weapon** — manually equipping a shield alone no longer gets yanked away.
- Fix: R sheath no longer unequips the shield in a HideHandItems prefix (that emptied both hands and made R pop weapons back out).
- Fix: drawing with R now triggers shield auto-equip reliably.
