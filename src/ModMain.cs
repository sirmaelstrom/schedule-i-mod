using MelonLoader;
using UnityEngine;

#if IL2CPP
using Il2CppScheduleOne;  // Verify this namespace with dnSpy!
using Il2CppInterop.Runtime.Injection;
#else
using ScheduleOne;  // Verify this namespace with dnSpy!
#endif

[assembly: MelonInfo(typeof(ScheduleIMod.ModMain), "ScheduleIMod", "0.1.0", "Sirmaelstrom")]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace ScheduleIMod
{
    public class ModMain : MelonMod
    {
        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("=================================");
            LoggerInstance.Msg("Schedule I Mod Loaded!");

            #if IL2CPP
            LoggerInstance.Msg("Running IL2CPP build");
            #else
            LoggerInstance.Msg("Running Mono build");
            #endif

            LoggerInstance.Msg($"MelonLoader Version: {BuildInfo.Version}");
            LoggerInstance.Msg($"Game Version: {Application.version}");
            LoggerInstance.Msg($"Unity Version: {Application.unityVersion}");
            LoggerInstance.Msg("=================================");

            // Initialize economy configuration
            Config.EconomyConfig.Initialize();
            LoggerInstance.Msg($"Economy config initialized - Cooldown multiplier: {Config.EconomyConfig.CooldownMultiplier}x");
        }

        public override void OnLateInitializeMelon()
        {
            LoggerInstance.Msg("Late initialization complete - patches should be active");
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            LoggerInstance.Msg($"Scene loaded: {sceneName} (index: {buildIndex})");
        }
    }
}
