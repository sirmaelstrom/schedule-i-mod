using HarmonyLib;
using MelonLoader;

#if IL2CPP
using Il2CppScheduleOne.Persistence;  // Verify namespace!
#else
using ScheduleOne.Persistence;  // Verify namespace!
#endif

namespace ScheduleIMod.Patches
{
    /// <summary>
    /// [EXAMPLE] Postfix patch that runs AFTER original method
    /// Can access and modify the return value with __result
    /// </summary>
    [HarmonyPatch(typeof(LoadManager), nameof(LoadManager.SaveGame))]
    public static class SaveGamePatch
    {
        // Postfix runs AFTER the original method
        static void Postfix(bool __result)
        {
            try
            {
                if (__result)
                {
                    MelonLogger.Msg("[EXAMPLE] Game saved successfully - triggering custom event");
                    // Custom post-save logic here
                }
                else
                {
                    MelonLogger.Warning("[EXAMPLE] Save failed");
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[EXAMPLE] Postfix error: {ex.Message}");
            }
        }
    }
}
