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
            out string invalidReason,
            out float remainingSeconds)
        {
            invalidReason = string.Empty;
            remainingSeconds = 0f;

            // Always respect pending offer check (game logic safety)
            if (hasPendingOffer)
            {
                invalidReason = "Customer already has a pending offer";
                return false;
            }

            // Scale the cooldown checks
            float scaledCooldown = ScaleCooldown(360f, cooldownMultiplier);

            if (timeSinceLastCompleted < scaledCooldown)
            {
                remainingSeconds = scaledCooldown - timeSinceLastCompleted;
                invalidReason = "Customer recently completed a deal.";
                return false;
            }

            if (timeSinceLastOffered < scaledCooldown && !hasPendingInstantDeal)
            {
                remainingSeconds = scaledCooldown - timeSinceLastOffered;
                invalidReason = "Already recently offered a deal.";
                return false;
            }

            return true;
        }
    }
}
