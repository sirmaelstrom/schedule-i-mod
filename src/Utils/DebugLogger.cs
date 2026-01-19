using MelonLoader;

namespace ScheduleIMod.Utils
{
    public static class DebugLogger
    {
        public static void Log(string message)
        {
            var level = Config.EconomyConfig.LogLevel;

            if (level == Config.EconomyConfig.DebugLogLevel.None)
                return;

            // Always log to console if enabled
            if (level >= Config.EconomyConfig.DebugLogLevel.Console)
            {
                MelonLogger.Msg($"[Economy] {message}");
            }

            // Also show toast if that level is set
            if (level == Config.EconomyConfig.DebugLogLevel.Toast)
            {
                ShowToast(message);
            }
        }

        private static void ShowToast(string message)
        {
            // TODO: Research game's notification system for in-game toasts
            // For now, just log to console as fallback
            MelonLogger.Msg($"[Economy Toast] {message}");
        }
    }
}
