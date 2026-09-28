using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cjayride.SmartShield
{
    internal static class ShieldService
    {
        /// <summary>True while we call Equip/Unequip so our patches do not re-enter.</summary>
        internal static bool InternalChange;

        /// <summary>Right-hand item was ranged/two-handed before the latest swap.</summary>
        private static bool _previousWasRangedOrTwoHanded;
        private static ItemDrop.ItemData _lastRight;
        private static bool _wasSheathed;

        /// <summary>Ignore hotbar-deselect unequip briefly after drawing hands (R).</summary>
        private static float _ignoreHotbarUnequipUntil;

        internal static void Baseline(Player player)
        {
            ItemDrop.ItemData right = player.GetRightItem();
            _lastRight = right;
            _wasSheathed = IsSheathed(player);
            _previousWasRangedOrTwoHanded = right?.m_shared != null && IsRangedOrTwoHanded(right.m_shared.m_itemType);
        }

        internal static void NotifyHandsShown()
        {
            _ignoreHotbarUnequipUntil = Time.unscaledTime + 0.35f;
            Player player = Player.m_localPlayer;
            if (player != null)
            {
                _lastRight = player.GetRightItem();
                _wasSheathed = IsSheathed(player);
            }
        }

        internal static void Tick(Player player)
        {
            ItemDrop.ItemData prevRight = _lastRight;
            ItemDrop.ItemData right = player.GetRightItem();
            ItemDrop.ItemData left = player.GetLeftItem();
            bool sheathed = IsSheathed(player);

            // Keep correcting while R-sheathed if a shield is still on hand/back.
            if (ModConfig.UnequipOnSheath.Value && sheathed && ShieldStillAttachedWhileSheathed(player))
            {
                _lastRight = right;
                _wasSheathed = true;
                Apply();
                return;
            }

            bool rightChanged = !ReferenceEquals(right, prevRight);
            bool sheathChanged = sheathed != _wasSheathed;
            if (!rightChanged && !sheathChanged)
            {
                return;
            }

            // Hotbar deselect only: had a one-hander, now empty hands (not R-sheath).
            // Do NOT unequip when the player manually equips a shield with empty main hand.
            if (ModConfig.UnequipOnHotbarDeselect.Value
                && Time.unscaledTime >= _ignoreHotbarUnequipUntil
                && right == null
                && !sheathed
                && prevRight != null
                && IsOneHandedWeapon(prevRight)
                && left?.m_shared != null
                && left.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shield)
            {
                _lastRight = right;
                _wasSheathed = sheathed;
                UnequipSilent(player, left);
                return;
            }

            if (right?.m_shared != null && IsRangedOrTwoHanded(right.m_shared.m_itemType))
            {
                _previousWasRangedOrTwoHanded = true;
            }

            _lastRight = right;
            _wasSheathed = sheathed;
            Apply();
        }

        internal static void Apply()
        {
            if (!ModConfig.Enabled.Value || InternalChange)
            {
                return;
            }

            Player player = Player.m_localPlayer;
            if (player == null || player.IsDead())
            {
                return;
            }

            try
            {
                ApplyInner(player);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("SmartShield apply failed: " + ex.Message);
            }
        }

        private static void ApplyInner(Player player)
        {
            ItemDrop.ItemData right = player.GetRightItem();
            ItemDrop.ItemData left = player.GetLeftItem();
            bool previousRanged = _previousWasRangedOrTwoHanded;
            _previousWasRangedOrTwoHanded = right?.m_shared != null && IsRangedOrTwoHanded(right.m_shared.m_itemType);

            // R sheath: weapon stashed in m_hiddenRightItem.
            if (IsSheathed(player))
            {
                if (ModConfig.UnequipOnSheath.Value)
                {
                    RetractShieldWhileSheathed(player);
                }

                return;
            }

            // Empty main hand (hotbar deselect / shield-only): do not auto-strip the
            // shield here. Tick handles hotbar deselect as a one-hander → empty transition.
            if (right == null)
            {
                return;
            }

            if (IsRangedOrTwoHanded(right.m_shared.m_itemType))
            {
                if (ModConfig.UnequipOnTwoHanded.Value && left != null &&
                    left.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shield)
                {
                    UnequipSilent(player, left);
                }

                return;
            }

            if (right == null || !IsOneHandedWeapon(right))
            {
                return;
            }

            if (!WeaponAllowed(right))
            {
                return;
            }

            bool wantShield = ModConfig.AutoEquipOnOneHanded.Value;
            if (!wantShield && ModConfig.EquipAfterRanged.Value && previousRanged)
            {
                wantShield = true;
            }

            if (!wantShield)
            {
                return;
            }

            // Off-hand must be empty or already a shield (never steal a torch, etc.).
            if (left != null && left.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Shield)
            {
                return;
            }

            if (left != null && left.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shield)
            {
                return;
            }

            ItemDrop.ItemData shield = SelectShield(player);
            if (shield == null)
            {
                return;
            }

            EquipSilent(player, shield);
        }

        internal static bool IsSheathed(Humanoid humanoid)
        {
            return humanoid != null && humanoid.GetRightItem() == null && humanoid.m_hiddenRightItem != null;
        }

        internal static bool ShieldStillAttachedWhileSheathed(Humanoid humanoid)
        {
            if (!IsSheathed(humanoid))
            {
                return false;
            }

            ItemDrop.ItemData left = humanoid.GetLeftItem();
            if (left?.m_shared != null && left.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shield)
            {
                return true;
            }

            ItemDrop.ItemData hiddenLeft = humanoid.m_hiddenLeftItem;
            return hiddenLeft?.m_shared != null &&
                   hiddenLeft.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shield;
        }

        /// <summary>
        /// Called from HideHandItems prefix/postfix and from the poll. Clears hand + back shield mesh.
        /// </summary>
        internal static void RetractShieldWhileSheathed(Humanoid humanoid)
        {
            if (humanoid == null)
            {
                return;
            }

            ItemDrop.ItemData left = humanoid.GetLeftItem();
            if (left?.m_shared != null && left.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shield)
            {
                UnequipSilent(humanoid, left);
            }

            ItemDrop.ItemData hiddenLeft = humanoid.m_hiddenLeftItem;
            if (hiddenLeft?.m_shared != null &&
                hiddenLeft.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shield)
            {
                humanoid.m_hiddenLeftItem = null;
            }

            // Always force the back-slot visual off. SetLeftBackItem alone only writes the
            // ZDO; UpdateEquipmentVisuals destroys m_leftBackItemInstance.
            VisEquipment vis = humanoid.m_visEquipment;
            if (vis == null)
            {
                return;
            }

            vis.SetLeftBackItem(0, 0, 0);
            if (vis.m_leftBackItemInstance != null)
            {
                UnityEngine.Object.Destroy(vis.m_leftBackItemInstance);
                vis.m_leftBackItemInstance = null;
                vis.m_currentLeftBackItemHash = 0;
                vis.m_currentLeftBackItemVariant = 0;
                vis.m_currentLeftBackItemQuality = 0;
            }

            humanoid.SetupVisEquipment(vis, false);
            vis.UpdateEquipmentVisuals();
        }

        internal static void UnequipSilentPublic(Humanoid player, ItemDrop.ItemData item)
        {
            UnequipSilent(player, item);
        }

        private static bool IsOneHandedWeapon(ItemDrop.ItemData item)
        {
            return item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.OneHandedWeapon;
        }

        private static bool IsRangedOrTwoHanded(ItemDrop.ItemData.ItemType type)
        {
            return type == ItemDrop.ItemData.ItemType.Bow
                || type == ItemDrop.ItemData.ItemType.TwoHandedWeapon
                || type == ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft
                || type == ItemDrop.ItemData.ItemType.Attach_Atgeir;
        }

        private static bool WeaponAllowed(ItemDrop.ItemData weapon)
        {
            string raw = ModConfig.AllowedWeaponTypes.Value;
            if (string.IsNullOrWhiteSpace(raw))
            {
                return true;
            }

            string prefab = PrefabName(weapon);
            string typeName = weapon.m_shared.m_itemType.ToString();
            string skill = weapon.m_shared.m_skillType.ToString();
            foreach (string token in SplitList(raw))
            {
                if (token.Equals(typeName, StringComparison.OrdinalIgnoreCase)
                    || token.Equals(skill, StringComparison.OrdinalIgnoreCase)
                    || token.Equals(prefab, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static ItemDrop.ItemData SelectShield(Player player)
        {
            List<ItemDrop.ItemData> items = player.GetInventory().GetAllItems();
            HashSet<string> banned = new HashSet<string>(SplitList(ModConfig.BlacklistShields.Value), StringComparer.OrdinalIgnoreCase);
            string mode = (ModConfig.SelectionMode.Value ?? "Best").Trim();
            string preferred = (ModConfig.PreferredShield.Value ?? "").Trim();

            ItemDrop.ItemData best = null;
            float bestBlock = float.MinValue;

            for (int i = 0; i < items.Count; i++)
            {
                ItemDrop.ItemData item = items[i];
                if (item?.m_shared == null || item.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Shield)
                {
                    continue;
                }

                string prefab = PrefabName(item);
                if (banned.Contains(prefab))
                {
                    continue;
                }

                if (mode.Equals("Preferred", StringComparison.OrdinalIgnoreCase) && preferred.Length > 0)
                {
                    if (prefab.Equals(preferred, StringComparison.OrdinalIgnoreCase))
                    {
                        return item;
                    }

                    continue;
                }

                if (mode.Equals("First", StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }

                float block = item.m_shared.m_blockPower + item.m_shared.m_timedBlockBonus;
                if (best == null || block > bestBlock)
                {
                    best = item;
                    bestBlock = block;
                }
            }

            // Preferred missing: fall back to highest block so the swap still works.
            if (best == null && mode.Equals("Preferred", StringComparison.OrdinalIgnoreCase))
            {
                return SelectFallbackBest(items, banned);
            }

            return best;
        }

        private static ItemDrop.ItemData SelectFallbackBest(List<ItemDrop.ItemData> items, HashSet<string> banned)
        {
            ItemDrop.ItemData best = null;
            float bestBlock = float.MinValue;
            for (int i = 0; i < items.Count; i++)
            {
                ItemDrop.ItemData item = items[i];
                if (item?.m_shared == null || item.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Shield)
                {
                    continue;
                }

                if (banned.Contains(PrefabName(item)))
                {
                    continue;
                }

                float block = item.m_shared.m_blockPower + item.m_shared.m_timedBlockBonus;
                if (best == null || block > bestBlock)
                {
                    best = item;
                    bestBlock = block;
                }
            }

            return best;
        }

        private static void EquipSilent(Humanoid player, ItemDrop.ItemData item)
        {
            InternalChange = true;
            try
            {
                player.EquipItem(item, false);
            }
            finally
            {
                InternalChange = false;
            }
        }

        private static void UnequipSilent(Humanoid player, ItemDrop.ItemData item)
        {
            InternalChange = true;
            try
            {
                player.UnequipItem(item, false);
            }
            finally
            {
                InternalChange = false;
            }
        }

        private static string PrefabName(ItemDrop.ItemData item)
        {
            if (item.m_dropPrefab != null)
            {
                return item.m_dropPrefab.name;
            }

            return item.m_shared.m_name ?? "";
        }

        private static List<string> SplitList(string raw)
        {
            List<string> list = new List<string>();
            if (string.IsNullOrWhiteSpace(raw))
            {
                return list;
            }

            string[] parts = raw.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                string t = parts[i].Trim();
                if (t.Length > 0)
                {
                    list.Add(t);
                }
            }

            return list;
        }
    }
}
