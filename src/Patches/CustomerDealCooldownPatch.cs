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
                // Game uses int for time values, convert to float
                float timeSinceLastCompleted = Convert.ToSingle(traverse.Property("TimeSinceLastDealCompleted").GetValue());
                float timeSinceInstantOffered = Convert.ToSingle(traverse.Property("TimeSinceInstantDealOffered").GetValue());
                object offeredContractInfo = traverse.Property<object>("OfferedContractInfo").Value;
                bool pendingInstantDeal = traverse.Field<bool>("pendingInstantDeal").Value;

                // Try to find customer name through object references
                string customerName = null;

                // Try to find Data or Info objects that might contain name
                try
                {
                    var dataObj = traverse.Field("data").GetValue();
                    if (dataObj != null)
                    {
                        var dataTraverse = Traverse.Create(dataObj);
                        customerName =
                            dataTraverse.Field("name").GetValue()?.ToString() ??
                            dataTraverse.Field("displayName").GetValue()?.ToString() ??
                            dataTraverse.Property("Name").GetValue()?.ToString();
                    }
                }
                catch { }

                // Fallback to hashcode-based ID
                if (string.IsNullOrEmpty(customerName))
                {
                    customerName = $"Customer_{__instance.GetHashCode()}";
                }

                Utils.DebugLogger.Log($"[{customerName}] OfferDealValid called - timeSinceLastCompleted={timeSinceLastCompleted:F1}s, multiplier={Config.EconomyConfig.CooldownMultiplier:F1}x");

                __result = Logic.CooldownLogic.ShouldAllowDeal(
                    timeSinceLastCompleted,
                    timeSinceInstantOffered,
                    offeredContractInfo != null,
                    pendingInstantDeal,
                    Config.EconomyConfig.CooldownMultiplier,
                    out invalidReason,
                    out float remainingSeconds
                );

                Utils.DebugLogger.Log($"[{customerName}] Result: allowed={__result}, message='{invalidReason}'");

#if IL2CPP
                // Custom countdown UI using OnGUI
                if (!__result && remainingSeconds > 0)
                {
                    ModMain.StartCooldownTracking(customerName, remainingSeconds);
                }
                else if (__result)
                {
                    ModMain.StopCooldownTracking(customerName);
                }
#endif

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
