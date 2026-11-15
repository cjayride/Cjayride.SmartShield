using HarmonyLib;
using UnityEngine;

namespace Cjayride.SmartShield
{
    [HarmonyPatch(typeof(GameCamera), nameof(GameCamera.UpdateMouseCapture))]
    internal static class PatchMouseCapture
    {
        private static void Postfix()
        {
            if (!OptionsMenu.IsOpen)
            {
                return;
            }

            ZCursor.LockState = CursorLockMode.None;
            ZCursor.Show();
        }
    }

    [HarmonyPatch(typeof(Player), "TakeInput")]
    internal static class PatchPlayerTakeInput
    {
        private static void Postfix(ref bool __result)
        {
            if (OptionsMenu.IsOpen)
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerController), "TakeInput", typeof(bool))]
    internal static class PatchControllerTakeInput
    {
        private static void Postfix(ref bool __result)
        {
            if (OptionsMenu.IsOpen)
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerController), "InInventoryEtc")]
    internal static class PatchControllerInInventoryEtc
    {
        private static void Postfix(ref bool __result)
        {
            if (OptionsMenu.IsOpen)
            {
                __result = true;
            }
        }
    }
}
