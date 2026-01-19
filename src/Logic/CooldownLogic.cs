namespace ScheduleIMod.Logic
{
    public static class CooldownLogic
    {
        public static float ScaleCooldown(float baseSeconds, float multiplier)
        {
            return baseSeconds * multiplier;
        }
    }
}
