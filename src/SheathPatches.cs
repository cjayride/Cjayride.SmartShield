using HarmonyLib;

namespace Cjayride.SmartShield
{
    internal static class PatchHelpers
    {
        internal static bool IsLocal(Humanoid humanoid)
        {
            return humanoid != null && Player.m_localPlayer != null && humanoid == Player.m_localPlayer;
        }

        internal static bool ModEnabled()
        {
            return ModConfig.Enabled != null && ModConfig.Enabled.Value;
        }

        internal static bool SheathUnequipEnabled()
        {
            return ModEnabled() && ModConfig.UnequipOnSheath != null && ModConfig.UnequipOnSheath.Value;
        }
    }

    /// <summary>
    /// Sheath (R) stashes the left item in m_hiddenLeftItem and draws it on the back.
    /// Clear that after hide; do not unequip the shield in Prefix (that can empty both
    /// hands and make the next R press ShowHandItems instead of hide).
    /// </summary>
    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.HideHandItems))]
    internal static class PatchHideHandItems
    {
        private static void Postfix(Humanoid __instance, bool onlyRightHand)
        {
            if (onlyRightHand || !PatchHelpers.IsLocal(__instance) || !PatchHelpers.SheathUnequipEnabled())
            {
                return;
            }

            ShieldService.RetractShieldWhileSheathed(__instance);
        }
    }

    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.ShowHandItems))]
    internal static class PatchShowHandItems
    {
        private static void Prefix(Humanoid __instance, bool onlyRightHand)
        {
            if (onlyRightHand || !PatchHelpers.IsLocal(__instance) || !PatchHelpers.SheathUnequipEnabled())
            {
                return;
            }

            ItemDrop.ItemData hiddenLeft = __instance.m_hiddenLeftItem;
            if (hiddenLeft?.m_shared != null &&
                hiddenLeft.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shield)
            {
                __instance.m_hiddenLeftItem = null;
            }
        }

        private static void Postfix(Humanoid __instance, bool onlyRightHand)
        {
            if (onlyRightHand || !PatchHelpers.IsLocal(__instance) || !PatchHelpers.ModEnabled())
            {
                return;
            }

            ShieldService.NotifyHandsShown();
            ShieldService.Apply();
        }
    }

    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.EquipItem))]
    internal static class PatchEquipItem
    {
        private static void Postfix(Humanoid __instance)
        {
            ShieldService.OnLocalEquipmentChanged(__instance);
        }
    }

    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UnequipItem))]
    internal static class PatchUnequipItem
    {
        private static void Prefix(Humanoid __instance, ItemDrop.ItemData item)
        {
            if (ShieldService.InternalChange || !PatchHelpers.ModEnabled() || !PatchHelpers.IsLocal(__instance))
            {
                return;
            }

            if (!IsThrowableOneHander(item))
            {
                return;
            }

            Attack current = __instance.m_currentAttack;
            if (__instance.InAttack() || (current != null && ReferenceEquals(current.m_weapon, item)))
            {
                ShieldService.NotifyThrownWeapon();
            }
        }

        private static void Postfix(Humanoid __instance)
        {
            ShieldService.OnLocalEquipmentChanged(__instance);
        }

        private static bool IsThrowableOneHander(ItemDrop.ItemData item)
        {
            if (item?.m_shared == null || item.m_shared.m_itemType != ItemDrop.ItemData.ItemType.OneHandedWeapon)
            {
                return false;
            }

            if (item.m_shared.m_skillType == Skills.SkillType.Spears)
            {
                return true;
            }

            return HasProjectile(item.m_shared.m_attack) || HasProjectile(item.m_shared.m_secondaryAttack);
        }

        private static bool HasProjectile(Attack attack)
        {
            return attack != null && attack.m_attackProjectile != null;
        }
    }

    [HarmonyPatch(typeof(Attack), nameof(Attack.OnAttackTrigger))]
    internal static class PatchOnAttackTrigger
    {
        private static void Prefix(Attack __instance)
        {
            if (__instance == null || !PatchHelpers.ModEnabled() || __instance.m_attackProjectile == null)
            {
                return;
            }

            Humanoid humanoid = __instance.m_character as Humanoid;
            if (!PatchHelpers.IsLocal(humanoid))
            {
                return;
            }

            ItemDrop.ItemData weapon = __instance.m_weapon;
            if (weapon?.m_shared == null || weapon.m_shared.m_itemType != ItemDrop.ItemData.ItemType.OneHandedWeapon)
            {
                return;
            }

            ShieldService.NotifyThrownWeapon();
        }
    }
}
