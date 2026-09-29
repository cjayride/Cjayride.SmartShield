using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace Cjayride.SmartShield
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGUID = "cjayride.smartshield";
        public const string PluginName = "SmartShield";
        public const string PluginVersion = "1.0.11";

        internal static Plugin Instance;
        internal static Harmony Harmony;
        internal static BepInEx.Logging.ManualLogSource Log => Instance.Logger;

        private float _seenPlayerAt = -1f;
        private float _nextPoll;
        private bool _baselined;
        private int _catchUpPolls;

        private void Awake()
        {
            Instance = this;
            ModConfig.Bind(Config);
            Harmony = new Harmony(PluginGUID);
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
            Logger.LogInfo("SmartShield " + PluginVersion + " loaded.");
        }

        private void Update()
        {
            if (!ModConfig.Enabled.Value)
            {
                return;
            }

            Player player = Player.m_localPlayer;
            if (player == null || player.IsDead())
            {
                _seenPlayerAt = -1f;
                _baselined = false;
                _catchUpPolls = 0;
                return;
            }

            if (_seenPlayerAt < 0f)
            {
                _seenPlayerAt = Time.unscaledTime;
                return;
            }

            // Brief idle so VisEquipment exists, but not so long the first sword draw is missed.
            if (Time.unscaledTime < _seenPlayerAt + 1.5f)
            {
                return;
            }

            if (Time.unscaledTime < _nextPoll)
            {
                return;
            }

            _nextPoll = Time.unscaledTime + 0.12f;

            if (!_baselined)
            {
                ShieldService.Baseline(player);
                _baselined = true;
                // Sword may already be out from during warmup — Tick would never see a change.
                ShieldService.Apply();
                _catchUpPolls = 10;
                return;
            }

            ShieldService.Tick(player);

            // Retry a few times in case the first EquipItem raced inventory/vis setup.
            if (_catchUpPolls > 0)
            {
                _catchUpPolls--;
                ShieldService.Apply();
            }
        }
    }
}
