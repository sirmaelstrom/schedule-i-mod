using HarmonyLib;
using MelonLoader;

#if IL2CPP
using Il2CppScheduleOne;  // Verify namespace!
#else
using ScheduleOne;  // Verify namespace!
#endif

namespace ScheduleIMod.Patches
{
#if FALSE  // Disabled until real game classes are identified with dnSpy
    /// <summary>
    /// [EXAMPLE] Prefix patch that modifies behavior BEFORE original method runs
    /// Returning false skips the original method entirely
    /// </summary>
    [HarmonyPatch(typeof(Customer), nameof(Customer.OfferDealValid))]
    public static class CustomerDealValidPatch
    {
        // Prefix runs BEFORE the original method
        // __result is ref parameter for the return value
        static bool Prefix(ref string invalidReason, ref bool __result)
        {
            try
            {
                // Force all deals to be valid (example: remove cooldown restrictions)
                invalidReason = string.Empty;
                __result = true;

                MelonLogger.Msg("[EXAMPLE] Deal validation bypassed");

                // Return false to skip original method
                // Return true to let original method run
                return false;
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[EXAMPLE] Patch error: {ex.Message}");
                return true; // Let original run on error
            }
        }
    }
#endif
}
