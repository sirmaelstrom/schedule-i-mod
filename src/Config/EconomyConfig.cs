using MelonLoader;

namespace ScheduleIMod.Config
{
    public static class EconomyConfig
    {
        private static MelonPreferences_Category _category;
        private static MelonPreferences_Entry<float> _cooldownMultiplier;
        private static MelonPreferences_Entry<DebugLogLevel> _debugLogLevel;

        public enum DebugLogLevel
        {
            None,
            Console,
            Toast
        }

        // Public accessors
        public static float CooldownMultiplier => _cooldownMultiplier.Value;
        public static DebugLogLevel LogLevel => _debugLogLevel.Value;

        // Calculated property for display
        public static float CurrentCooldownSeconds => 360f * CooldownMultiplier;

        public static void Initialize()
        {
            _category = MelonPreferences.CreateCategory("Economy");
            _category.SetFilePath("UserData/MelonPreferences.cfg");

            _cooldownMultiplier = _category.CreateEntry(
                "DealCooldownMultiplier",
                1.0f,
                "Deal Cooldown Multiplier",
                "Scales customer deal cooldowns (0.0 = instant, 1.0 = vanilla 360s, 2.0 = double)"
            );

            _debugLogLevel = _category.CreateEntry(
                "DebugLogLevel",
                DebugLogLevel.None,
                "Debug Logging",
                "None = silent, Console = log to console, Toast = console + in-game messages"
            );

            // Hot reload callbacks
            _cooldownMultiplier.OnEntryValueChanged.Subscribe(OnCooldownMultiplierChanged);
            _debugLogLevel.OnEntryValueChanged.Subscribe(OnDebugLogLevelChanged);
        }

        private static void OnCooldownMultiplierChanged(float oldValue, float newValue)
        {
            Utils.DebugLogger.Log($"Deal cooldown changed: {oldValue:F1}x -> {newValue:F1}x ({CurrentCooldownSeconds:F0}s)");
        }

        private static void OnDebugLogLevelChanged(DebugLogLevel oldValue, DebugLogLevel newValue)
        {
            Utils.DebugLogger.Log($"Debug log level changed: {oldValue} -> {newValue}");
        }
    }
}
