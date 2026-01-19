namespace ScheduleIMod.Logic
{
    public static class CooldownLogic
    {
        public static float ScaleCooldown(float baseSeconds, float multiplier)
        {
            return baseSeconds * multiplier;
        }

        public static bool ShouldAllowDeal(
            float timeSinceLastCompleted,
            float timeSinceLastOffered,
            bool hasPendingOffer,
            bool hasPendingInstantDeal,
            float cooldownMultiplier,
            out string invalidReason)
        {
            invalidReason = string.Empty;

            // Always respect pending offer check (game logic safety)
            if (hasPendingOffer)
            {
                invalidReason = "Customer already has a pending offer";
                return false;
            }

            // Placeholder for cooldown checks (will implement next)
            return true;
        }
    }
}
