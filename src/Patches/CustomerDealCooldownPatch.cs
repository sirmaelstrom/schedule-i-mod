using HarmonyLib;
using MelonLoader;
using System;
using System.Reflection;

namespace ScheduleIMod.Patches
{
    [HarmonyPatch]
    public static class CustomerDealCooldownPatch
    {
        static MethodBase TargetMethod()
        {
#if IL2CPP
            var customerType = AccessTools.TypeByName("Il2CppScheduleOne.Economy.Customer");
#else
            var customerType = AccessTools.TypeByName("ScheduleOne.Economy.Customer");
#endif
            return AccessTools.Method(customerType, "OfferDealValid");
        }

        static bool Prefix(object __instance, ref string invalidReason, ref bool __result)
        {
            try
            {
                var traverse = Traverse.Create(__instance);

                // Access all fields using Traverse to avoid compile-time Customer type resolution
                float timeSinceLastCompleted = traverse.Property<float>("TimeSinceLastDealCompleted").Value;
                float timeSinceInstantOffered = traverse.Property<float>("TimeSinceInstantDealOffered").Value;
                object offeredContractInfo = traverse.Property<object>("OfferedContractInfo").Value;
                bool pendingInstantDeal = traverse.Field<bool>("pendingInstantDeal").Value;

                __result = Logic.CooldownLogic.ShouldAllowDeal(
                    timeSinceLastCompleted,
                    timeSinceInstantOffered,
                    offeredContractInfo != null,
                    pendingInstantDeal,
                    Config.EconomyConfig.CooldownMultiplier,
                    out invalidReason
                );

                Utils.DebugLogger.Log($"Deal check: allowed={__result}, reason='{invalidReason}'");

                return false; // Skip original method
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"CustomerDealCooldownPatch error: {ex}");
                return true; // Run original on error
            }
        }
    }
}
