using HarmonyLib;

namespace Cjayride.SmartShield
{
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
            if (onlyRightHand || !IsLocal(__instance) || !SheathUnequipEnabled())
            {
                return;
            }

            ShieldService.RetractShieldWhileSheathed(__instance);
        }

        private static bool IsLocal(Humanoid humanoid)
        {
            return humanoid != null && Player.m_localPlayer != null && humanoid == Player.m_localPlayer;
        }

        private static bool SheathUnequipEnabled()
        {
            return ModConfig.Enabled != null && ModConfig.Enabled.Value
                && ModConfig.UnequipOnSheath != null && ModConfig.UnequipOnSheath.Value;
        }
    }

    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.ShowHandItems))]
    internal static class PatchShowHandItems
    {
        private static void Prefix(Humanoid __instance, bool onlyRightHand)
        {
            if (onlyRightHand || !IsLocal(__instance) || !SheathUnequipEnabled())
            {
                return;
            }

            // Don't restore a stashed shield when drawing (shield stays in inventory for auto-equip).
            ItemDrop.ItemData hiddenLeft = __instance.m_hiddenLeftItem;
            if (hiddenLeft?.m_shared != null &&
                hiddenLeft.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shield)
            {
                __instance.m_hiddenLeftItem = null;
            }
        }

        private static void Postfix(Humanoid __instance, bool onlyRightHand)
        {
            if (onlyRightHand || !IsLocal(__instance))
            {
                return;
            }

            if (ModConfig.Enabled == null || !ModConfig.Enabled.Value)
            {
                return;
            }

            // Weapon is back out — auto-equip shield if configured.
            ShieldService.NotifyHandsShown();
            ShieldService.Apply();
        }

        private static bool IsLocal(Humanoid humanoid)
        {
            return humanoid != null && Player.m_localPlayer != null && humanoid == Player.m_localPlayer;
        }

        private static bool SheathUnequipEnabled()
        {
            return ModConfig.Enabled != null && ModConfig.Enabled.Value
                && ModConfig.UnequipOnSheath != null && ModConfig.UnequipOnSheath.Value;
        }
    }
}
