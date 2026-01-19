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
            // Category identifier must start with mod name for Mod Manager detection
            _category = MelonPreferences.CreateCategory("ScheduleIMod_Economy", "Economy Settings");
            // Let MelonLoader handle the default preferences file path

            _cooldownMultiplier = _category.CreateEntry(
                "DealCooldownMultiplier",
                1.0f,
                "Deal Cooldown Multiplier",
                "Scales customer deal cooldowns (0.0 = instant, 1.0 = vanilla 360s, 2.0 = double)",
                false,
                false
            );

            _debugLogLevel = _category.CreateEntry(
                "DebugLogLevel",
                DebugLogLevel.None,
                "Debug Log Level",
                "Controls debug logging: None = silent, Console = MelonLoader console, Toast = console + in-game notifications",
                false,
                false
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
