using BepInEx.Configuration;

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

        internal static ConfigEntry<string> AllowedWeaponTypes;
        internal static ConfigEntry<string> BlacklistShields;

        internal static void Bind(ConfigFile config)
        {
            Enabled = config.Bind("General", "Enabled", true, "Master switch. No messages are ever shown.");

            AutoEquipOnOneHanded = config.Bind("Behavior", "AutoEquipOnOneHanded", true,
                "Equip a shield whenever a one-handed weapon is equipped.");
            EquipAfterRanged = config.Bind("Behavior", "EquipAfterRanged", true,
                "Also equip a shield after switching from a bow/crossbow (or other ranged) to a one-handed weapon.");
            UnequipOnTwoHanded = config.Bind("Behavior", "UnequipOnTwoHanded", true,
                "Unequip the shield when a two-handed or ranged weapon is equipped.");
            UnequipOnSheath = config.Bind("Behavior", "UnequipOnSheath", true,
                "Unequip the shield when weapons are sheathed with R (clears the back slot).");
            UnequipOnHotbarDeselect = config.Bind("Behavior", "UnequipOnHotbarDeselect", true,
                "Unequip the shield when you deselect a one-handed weapon on the hotbar (press the same slot again). Does not unequip if you manually equip a shield alone.");

            SelectionMode = config.Bind("ShieldSelection", "SelectionMode", "Best",
                "Best = highest block power. First = first shield in inventory. Preferred = use PreferredShield.");
            PreferredShield = config.Bind("ShieldSelection", "PreferredShield", "",
                "Prefab name used when SelectionMode is Preferred (example: ShieldWood).");

            AllowedWeaponTypes = config.Bind("Filters", "AllowedWeaponTypes", "",
                "Comma-separated ItemType, skill, or prefab names. Empty = all one-handed weapons.");
            BlacklistShields = config.Bind("Filters", "BlacklistShields", "",
                "Comma-separated shield prefab names that must never be auto-equipped.");
        }
    }
}
