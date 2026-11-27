using System.Collections;
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
        public const string PluginVersion = "1.0.21";

        internal static Plugin Instance;
        internal static Harmony Harmony;
        internal static BepInEx.Logging.ManualLogSource Log => Instance.Logger;

        private Coroutine _deferredApply;

        private float _seenPlayerAt = -1f;
        private float _nextPoll;
        private bool _baselined;
        private int _catchUpPolls;

        private void Awake()
        {
            Instance = this;
            ModConfig.Bind(Config);
            Harmony = new Harmony(PluginGUID);
            Harmony.PatchAll(typeof(PatchHideHandItems));
            Harmony.PatchAll(typeof(PatchShowHandItems));
            Harmony.PatchAll(typeof(PatchEquipItem));
            Harmony.PatchAll(typeof(PatchUnequipItem));
            Harmony.PatchAll(typeof(PatchOnAttackTrigger));
            Harmony.PatchAll(typeof(PatchPlayerTakeInput));
            Harmony.PatchAll(typeof(PatchControllerTakeInput));
            Harmony.PatchAll(typeof(PatchControllerInInventoryEtc));
            Harmony.PatchAll(typeof(PatchMouseCapture));
            Logger.LogInfo("SmartShield " + PluginVersion + " loaded.");
        }

        internal static void ScheduleApplyAfterEquip()
        {
            Plugin host = Instance;
            if (host == null || !ModConfig.Enabled.Value)
            {
                return;
            }

            if (host._deferredApply != null)
            {
                host.StopCoroutine(host._deferredApply);
            }

            host._deferredApply = host.StartCoroutine(host.ApplyAfterEquipFrames());
        }

        private IEnumerator ApplyAfterEquipFrames()
        {
            yield return null;
            Player player = Player.m_localPlayer;
            if (player != null && !player.IsDead() && !ShieldService.InternalChange)
            {
                ShieldService.ApplyAfterEquipmentSettled(player);
            }

            yield return null;
            player = Player.m_localPlayer;
            if (player != null && !player.IsDead() && !ShieldService.InternalChange)
            {
                ShieldService.ApplyAfterEquipmentSettled(player);
            }

            _deferredApply = null;
        }

        private void Update()
        {
            OptionsMenu.Tick();

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

            if (Time.unscaledTime < _seenPlayerAt + 1.5f)
            {
                return;
            }

            if (!_baselined)
            {
                ShieldService.Baseline(player);
                _baselined = true;
                ShieldService.Apply();
                _catchUpPolls = 3;
                _nextPoll = Time.unscaledTime + 0.25f;
                return;
            }

            if (_catchUpPolls > 0)
            {
                if (Time.unscaledTime < _nextPoll)
                {
                    return;
                }

                _nextPoll = Time.unscaledTime + 0.25f;
                _catchUpPolls--;
                ShieldService.Tick(player);
                return;
            }

            if (Time.unscaledTime < _nextPoll)
            {
                return;
            }

            _nextPoll = Time.unscaledTime + 0.75f;
            ShieldService.Tick(player);
        }

        private void OnGUI()
        {
            OptionsMenu.Draw();
        }
    }
}
