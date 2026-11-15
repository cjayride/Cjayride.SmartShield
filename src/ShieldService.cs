using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cjayride.SmartShield
{
    internal static class ShieldService
    {
        internal static bool InternalChange;

        private static bool _previousWasRangedOrTwoHanded;
        private static ItemDrop.ItemData _lastRight;
        private static bool _wasSheathed;
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

        /// <summary>Keep the shield when a spear (or other throwable one-hander) leaves the right hand mid-throw.</summary>
        internal static void NotifyThrownWeapon()
        {
            _ignoreHotbarUnequipUntil = Time.unscaledTime + 1.0f;
        }

        internal static void OnLocalEquipmentChanged(Humanoid humanoid)
        {
            if (InternalChange || !PatchHelpers.ModEnabled() || !PatchHelpers.IsLocal(humanoid))
            {
                return;
            }

            Player player = humanoid as Player;
            if (player == null)
            {
                return;
            }

            Tick(player);
        }

        internal static void Tick(Player player)
        {
            ItemDrop.ItemData prevRight = _lastRight;
            ItemDrop.ItemData right = player.GetRightItem();
            ItemDrop.ItemData left = player.GetLeftItem();
            bool sheathed = IsSheathed(player);

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

        internal static void ReapplyFromMenu()
        {
            Player player = Player.m_localPlayer;
            if (player == null || player.IsDead())
            {
                return;
            }

            ItemDrop.ItemData left = player.GetLeftItem();
            if (left?.m_shared != null && left.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shield)
            {
                UnequipSilent(player, left);
            }

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

            if (IsSheathed(player))
            {
                if (ModConfig.UnequipOnSheath.Value)
                {
                    RetractShieldWhileSheathed(player);
                }

                return;
            }

            if (right == null)
            {
                return;
            }

            ItemDrop.ItemData.ItemType rightType = right.m_shared.m_itemType;
            if (IsRangedOrTwoHanded(rightType))
            {
                if (ModConfig.UnequipOnTwoHanded.Value && left?.m_shared != null &&
                    left.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shield)
                {
                    UnequipSilent(player, left);
                }

                return;
            }

            if (rightType != ItemDrop.ItemData.ItemType.OneHandedWeapon)
            {
                return;
            }

            if (!WeaponAllowed(right))
            {
                return;
            }

            if (!ModConfig.EquipShieldWithAxe.Value && IsOneHandedAxe(right))
            {
                if (left?.m_shared != null && left.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shield)
                {
                    UnequipSilent(player, left);
                }

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

            if (left != null)
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
            if (hiddenLeft?.m_shared != null &&
                hiddenLeft.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shield)
            {
                return true;
            }

            VisEquipment vis = humanoid.m_visEquipment;
            return vis != null && (vis.m_leftBackItemInstance != null || vis.m_currentLeftBackItemHash != 0);
        }

        internal static void RetractShieldWhileSheathed(Humanoid humanoid)
        {
            if (humanoid == null)
            {
                return;
            }

            bool changed = false;
            ItemDrop.ItemData left = humanoid.GetLeftItem();
            if (left?.m_shared != null && left.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shield)
            {
                UnequipSilent(humanoid, left);
                changed = true;
            }

            ItemDrop.ItemData hiddenLeft = humanoid.m_hiddenLeftItem;
            if (hiddenLeft?.m_shared != null &&
                hiddenLeft.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shield)
            {
                humanoid.m_hiddenLeftItem = null;
                changed = true;
            }

            VisEquipment vis = humanoid.m_visEquipment;
            if (vis == null)
            {
                return;
            }

            bool hasBackVisual = vis.m_leftBackItemInstance != null || vis.m_currentLeftBackItemHash != 0;
            if (!changed && !hasBackVisual)
            {
                return;
            }

            vis.SetLeftBackItem(0, 0, 0);
            if (vis.m_leftBackItemInstance != null)
            {
                UnityEngine.Object.Destroy(vis.m_leftBackItemInstance);
                vis.m_leftBackItemInstance = null;
            }

            vis.m_currentLeftBackItemHash = 0;
            vis.m_currentLeftBackItemVariant = 0;
            vis.m_currentLeftBackItemQuality = 0;
            vis.UpdateEquipmentVisuals();
        }

        private static bool IsOneHandedWeapon(ItemDrop.ItemData item)
        {
            return item?.m_shared != null && item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.OneHandedWeapon;
        }

        private static bool IsOneHandedAxe(ItemDrop.ItemData item)
        {
            return item?.m_shared != null && item.m_shared.m_skillType == Skills.SkillType.Axes;
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
            string[] tokens = ModConfig.AllowedWeaponTokens;
            if (tokens.Length == 0)
            {
                return true;
            }

            string prefab = PrefabName(weapon);
            string typeName = weapon.m_shared.m_itemType.ToString();
            string skill = weapon.m_shared.m_skillType.ToString();
            for (int i = 0; i < tokens.Length; i++)
            {
                string token = tokens[i];
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
            Inventory inventory = player.GetInventory();
            if (inventory == null)
            {
                return null;
            }

            List<ItemDrop.ItemData> items = inventory.m_inventory;
            if (items == null || items.Count == 0)
            {
                return null;
            }

            HashSet<string> banned = ModConfig.BlacklistSet;
            bool hasBan = banned != null && banned.Count > 0;
            string mode = ModConfig.SelectionModeNormalized;
            string preferred = ModConfig.PreferredShieldNormalized;
            bool prefer = mode.Equals("Preferred", StringComparison.OrdinalIgnoreCase) && preferred.Length > 0;
            bool first = mode.Equals("First", StringComparison.OrdinalIgnoreCase);

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
                if (hasBan && banned.Contains(prefab))
                {
                    continue;
                }

                if (prefer)
                {
                    if (prefab.Equals(preferred, StringComparison.OrdinalIgnoreCase))
                    {
                        return item;
                    }

                    continue;
                }

                if (first)
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

            if (best == null && prefer)
            {
                return SelectFallbackBest(items, banned, hasBan);
            }

            return best;
        }

        private static ItemDrop.ItemData SelectFallbackBest(List<ItemDrop.ItemData> items, HashSet<string> banned, bool hasBan)
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

                if (hasBan && banned.Contains(PrefabName(item)))
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
    }
}
