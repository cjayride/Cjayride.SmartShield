using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;

namespace Cjayride.SmartShield
{
    internal static class ModConfig
    {
        internal static ConfigEntry<bool> Enabled;

        internal static ConfigEntry<bool> AutoEquipOnOneHanded;
        internal static ConfigEntry<bool> EquipAfterRanged;
        internal static ConfigEntry<bool> UnequipOnTwoHanded;
        internal static ConfigEntry<bool> UnequipOnSheath;
        internal static ConfigEntry<bool> UnequipOnHotbarDeselect;

        internal static ConfigEntry<string> SelectionMode;
        internal static ConfigEntry<string> PreferredShield;

        internal static ConfigEntry<bool> EquipShieldWithAxe;
        internal static ConfigEntry<KeyboardShortcut> MenuHotkey;

        internal static ConfigEntry<string> AllowedWeaponTypes;
        internal static ConfigEntry<string> BlacklistShields;

        internal static string[] AllowedWeaponTokens = Array.Empty<string>();
        internal static HashSet<string> BlacklistSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        internal static string SelectionModeNormalized = "First";
        internal static string PreferredShieldNormalized = "";

        internal static void Bind(ConfigFile config)
        {
            Enabled = config.Bind("General", "Enabled", true, "Master switch. No messages are ever shown.");

            AutoEquipOnOneHanded = config.Bind("Behavior", "AutoEquipOnOneHanded", true,
                "Equip a shield whenever a one-handed weapon is equipped.");
            EquipAfterRanged = config.Bind("Behavior", "EquipAfterRanged", true,
                "Also equip a shield after switching from a bow/crossbow (or other ranged) to a one-handed weapon.");
            UnequipOnTwoHanded = config.Bind("Behavior", "UnequipOnTwoHanded", true,
                "Unequip the shield when a two-handed or ranged weapon is equipped.");
            UnequipOnSheath = config.Bind("Behavior", "UnequipOnSheath", false,
                "When true, stow the shield in inventory on R-sheath (weapon-only on your back). When false, vanilla dual back display (weapon + shield).");
            UnequipOnHotbarDeselect = config.Bind("Behavior", "UnequipOnHotbarDeselect", true,
                "Unequip the shield when you deselect a one-handed weapon on the hotbar (press the same slot again). Does not unequip if you manually equip a shield alone.");

            SelectionMode = config.Bind("ShieldSelection", "SelectionMode", "First",
                "Best = highest block power. First = first shield in inventory. Preferred = use PreferredShield.");
            PreferredShield = config.Bind("ShieldSelection", "PreferredShield", "",
                "Prefab name used when SelectionMode is Preferred (example: ShieldWood).");
            EquipShieldWithAxe = config.Bind("ShieldSelection", "EquipShieldWithAxe", false,
                "When false, do not auto-equip a shield with a one-handed axe (tree chopping).");
            MenuHotkey = config.Bind("UI", "MenuHotkey", new KeyboardShortcut(KeyCode.S, KeyCode.LeftAlt),
                "Opens the SmartShield popup (selection mode and axe shield toggle). Default Alt+S.");

            AllowedWeaponTypes = config.Bind("Filters", "AllowedWeaponTypes", "",
                "Comma-separated ItemType, skill, or prefab names. Empty = all one-handed weapons.");
            BlacklistShields = config.Bind("Filters", "BlacklistShields", "",
                "Comma-separated shield prefab names that must never be auto-equipped.");

            AllowedWeaponTypes.SettingChanged += (_, __) => RebuildCaches();
            BlacklistShields.SettingChanged += (_, __) => RebuildCaches();
            SelectionMode.SettingChanged += (_, __) => RebuildCaches();
            PreferredShield.SettingChanged += (_, __) => RebuildCaches();
            RebuildCaches();
        }

        internal static void RebuildCaches()
        {
            AllowedWeaponTokens = SplitList(AllowedWeaponTypes.Value);
            BlacklistSet = new HashSet<string>(SplitList(BlacklistShields.Value), StringComparer.OrdinalIgnoreCase);
            SelectionModeNormalized = (SelectionMode.Value ?? "Best").Trim();
            PreferredShieldNormalized = (PreferredShield.Value ?? "").Trim();
        }

        internal static string[] SplitList(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return Array.Empty<string>();
            }

            string[] parts = raw.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            int count = 0;
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = parts[i].Trim();
                if (parts[i].Length > 0)
                {
                    count++;
                }
            }

            if (count == 0)
            {
                return Array.Empty<string>();
            }

            if (count == parts.Length)
            {
                return parts;
            }

            string[] trimmed = new string[count];
            int n = 0;
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Length > 0)
                {
                    trimmed[n++] = parts[i];
                }
            }

            return trimmed;
        }
    }
}
