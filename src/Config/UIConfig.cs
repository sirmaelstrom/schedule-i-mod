using MelonLoader;
using UnityEngine;

namespace ScheduleIMod.Config
{
    public static class UIConfig
    {
        private static MelonPreferences_Category _category;
        private static MelonPreferences_Entry<float> _windowX;
        private static MelonPreferences_Entry<float> _windowY;
        private static MelonPreferences_Entry<bool> _showWindow;
        private static MelonPreferences_Entry<KeyCode> _toggleKey;

        // Public accessors
        public static float WindowX
        {
            get => _windowX.Value;
            set => _windowX.Value = value;
        }

        public static float WindowY
        {
            get => _windowY.Value;
            set => _windowY.Value = value;
        }

        public static void SaveWindowPosition()
        {
            _category.SaveToFile();
        }

        public static bool ShowWindow
        {
            get => _showWindow.Value;
            set
            {
                _showWindow.Value = value;
                _category.SaveToFile();
            }
        }

        public static KeyCode ToggleKey => _toggleKey.Value;

        public static void Initialize()
        {
            _category = MelonPreferences.CreateCategory("ScheduleIMod_UI", "UI Settings");

            _windowX = _category.CreateEntry(
                "CooldownWindowX",
                20f,
                "Cooldown Window X Position",
                "Horizontal position of the cooldown tracker window",
                false,
                false
            );

            _windowY = _category.CreateEntry(
                "CooldownWindowY",
                20f,
                "Cooldown Window Y Position",
                "Vertical position of the cooldown tracker window",
                false,
                false
            );

            _showWindow = _category.CreateEntry(
                "ShowCooldownWindow",
                true,
                "Show Cooldown Window",
                "Whether the cooldown tracker window is visible",
                false,
                false
            );

            _toggleKey = _category.CreateEntry(
                "ToggleWindowKey",
                KeyCode.LeftBracket,
                "Toggle Window Keybind",
                "Key to toggle the cooldown tracker window visibility (default: [)",
                false,
                false
            );
        }
    }
}
