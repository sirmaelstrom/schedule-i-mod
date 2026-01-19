using MelonLoader;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#if IL2CPP
using Il2CppScheduleOne;  // Verify this namespace with dnSpy!
using Il2CppInterop.Runtime.Injection;
#else
using ScheduleOne;  // Verify this namespace with dnSpy!
#endif

[assembly: MelonInfo(typeof(ScheduleIMod.ModMain), "ScheduleIMod", "0.1.0", "Sirmaelstrom")]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace ScheduleIMod
{
    public class ModMain : MelonMod
    {
#if IL2CPP
        private static Dictionary<string, float> _customerCooldowns = new Dictionary<string, float>();
        private static Rect _windowRect;
        private static Vector2 _scrollPosition;
        private static GUIStyle _windowStyle;
        private static GUIStyle _labelStyle;
        private static GUIStyle _headerStyle;
        private static bool _stylesInitialized = false;
        private static readonly int _windowId = 1001;
        private const float WINDOW_WIDTH = 300f;
        private const float WINDOW_HEIGHT = 200f;
        private static Vector2 _lastSavedPosition;
        private static bool _forceShow = false; // Override config to force immediate visibility
        private static float _lastOnGuiTime = 0f; // Track when OnGUI was last called
        private static float _debugEventLogStart = 0f; // Track when to start verbose event logging
        private static bool _verboseEventLogging = false; // Enable detailed event logging
#endif

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("=================================");
            LoggerInstance.Msg("Schedule I Mod Loaded!");

            #if IL2CPP
            LoggerInstance.Msg("Running IL2CPP build");
            #else
            LoggerInstance.Msg("Running Mono build");
            #endif

            LoggerInstance.Msg($"MelonLoader Version: {BuildInfo.Version}");
            LoggerInstance.Msg($"Game Version: {Application.version}");
            LoggerInstance.Msg($"Unity Version: {Application.unityVersion}");
            LoggerInstance.Msg("=================================");

            // Initialize economy configuration
            Config.EconomyConfig.Initialize();
            LoggerInstance.Msg($"Economy config initialized - Cooldown multiplier: {Config.EconomyConfig.CooldownMultiplier}x");

#if IL2CPP
            // Initialize UI configuration
            Config.UIConfig.Initialize();
            LoggerInstance.Msg($"UI config initialized - Toggle key: {Config.UIConfig.ToggleKey}");
#endif
        }

        public override void OnLateInitializeMelon()
        {
            LoggerInstance.Msg("Late initialization complete - patches should be active");
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            LoggerInstance.Msg($"Scene loaded: {sceneName} (index: {buildIndex})");

#if IL2CPP
            // Clear cooldowns when leaving Main scene
            if (sceneName.ToLower() != "main" && _customerCooldowns.Count > 0)
            {
                _customerCooldowns.Clear();
                Config.UIConfig.ShowWindow = false;
                _forceShow = false;
                MelonLogger.Msg("[CooldownTracker] Cleared all cooldowns (scene changed)");
            }

            // Initialize window rect from config on scene load
            _windowRect = new Rect(Config.UIConfig.WindowX, Config.UIConfig.WindowY, WINDOW_WIDTH, WINDOW_HEIGHT);
            _lastSavedPosition = new Vector2(Config.UIConfig.WindowX, Config.UIConfig.WindowY);
            MelonLogger.Msg($"[CooldownTracker] Window initialized at ({Config.UIConfig.WindowX:F0}, {Config.UIConfig.WindowY:F0}), Screen: {Screen.width}x{Screen.height}");
#endif
        }

#if IL2CPP
        public override void OnUpdate()
        {
            // Handle toggle keybind
            if (Input.GetKeyDown(Config.UIConfig.ToggleKey))
            {
                bool newState = !Config.UIConfig.ShowWindow;
                Config.UIConfig.ShowWindow = newState;
                _forceShow = newState; // Also update force flag for immediate effect
                LoggerInstance.Msg($"Cooldown tracker window: {(newState ? "shown" : "hidden")}");
            }

            // Clean up expired cooldowns
            var expiredCustomers = _customerCooldowns
                .Where(kvp => kvp.Value <= Time.time)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var customer in expiredCustomers)
            {
                _customerCooldowns.Remove(customer);
                MelonLogger.Msg($"[CooldownTracker] Auto-removed {customer} (cooldown expired)");
            }
        }

        public override void OnGUI()
        {
            // Verbose event logging for first 5 seconds after window shown
            if (_verboseEventLogging && Time.time - _debugEventLogStart < 5f)
            {
                var eventType = Event.current?.type.ToString() ?? "null";
                MelonLogger.Msg($"[CooldownTracker] OnGUI - eventType={eventType}, count={_customerCooldowns.Count}");
            }
            else if (_verboseEventLogging)
            {
                _verboseEventLogging = false;
                MelonLogger.Msg($"[CooldownTracker] Verbose event logging ended");
            }

            // Track OnGUI calls when force-showing (throttled to once per second)
            if (_forceShow && Time.time - _lastOnGuiTime > 1f)
            {
                _lastOnGuiTime = Time.time;
                var eventType = Event.current?.type.ToString() ?? "null";
                MelonLogger.Msg($"[CooldownTracker] OnGUI called - forceShow={_forceShow}, count={_customerCooldowns.Count}, configShow={Config.UIConfig.ShowWindow}, eventType={eventType}");
            }

            // Early exit if no cooldowns to track
            if (_customerCooldowns.Count == 0)
            {
                _forceShow = false; // Reset force flag when no cooldowns
                return;
            }

            // Check if window should be shown (either from config or forced)
            bool shouldShow = _forceShow || Config.UIConfig.ShowWindow;

            if (!shouldShow)
            {
                if (_forceShow)
                    MelonLogger.Msg($"[CooldownTracker] OnGUI - shouldShow is FALSE but forceShow is TRUE!");
                return;
            }

            // Check GUI state
            if (!GUI.enabled)
            {
                MelonLogger.Msg($"[CooldownTracker] OnGUI - GUI.enabled is FALSE!");
                return;
            }

            // Initialize styles on first use
            if (!_stylesInitialized)
            {
                InitializeStyles();
            }

            // Force window to render on top of everything (negative depth renders later)
            int previousDepth = GUI.depth;
            GUI.depth = -1000;

            // WORKAROUND: Use GUI.Box instead of GUI.Window since Box renders immediately
            // Log once to confirm this code path is executing
            if (_verboseEventLogging && Time.time - _debugEventLogStart < 1f)
            {
                MelonLogger.Msg("[CooldownTracker] Using GUI.Box rendering (not GUI.Window)");
            }

            // Save current color
            Color savedColor = GUI.color;

            // Draw background box with BRIGHT COLOR (like the red test box that worked)
            GUI.color = new Color(0.2f, 0.2f, 0.2f, 0.95f); // Dark gray, mostly opaque
            GUI.Box(_windowRect, "");

            // Draw title bar (clickable for dragging) with BRIGHT COLOR
            Rect titleBarRect = new Rect(_windowRect.x, _windowRect.y, _windowRect.width, 20);
            GUI.color = new Color(0.3f, 0.3f, 0.8f, 0.95f); // Blue-ish, mostly opaque
            GUI.Box(titleBarRect, "Deal Cooldowns");

            // Reset color for text
            GUI.color = Color.white;

            // Handle dragging
            if (Event.current.type == EventType.MouseDown && titleBarRect.Contains(Event.current.mousePosition))
            {
                // Start drag - will be handled by MouseDrag events
            }
            else if (Event.current.type == EventType.MouseDrag && Input.GetMouseButton(0))
            {
                _windowRect.x += Event.current.delta.x;
                _windowRect.y += Event.current.delta.y;
            }

            // Draw content area
            float currentY = _windowRect.y + 25f;

            // Draw header with NO STYLE
            GUI.Label(new Rect(_windowRect.x + 10, currentY, _windowRect.width - 20, 20),
                     $"Customers: {_customerCooldowns.Count}");
            currentY += 25f;

            // Draw cooldown list
            var sortedCooldowns = _customerCooldowns.OrderBy(kvp => kvp.Value).ToList();
            float itemY = currentY;

            foreach (var kvp in sortedCooldowns)
            {
                float remainingTime = kvp.Value - Time.time;
                if (remainingTime > 0)
                {
                    int seconds = Mathf.CeilToInt(remainingTime);
                    string displayText = $"⏱ {kvp.Key}: {seconds}s";
                    GUI.Label(new Rect(_windowRect.x + 15, itemY, _windowRect.width - 30, 20), displayText);
                    itemY += 25f;
                }
            }

            // Restore previous depth and color
            GUI.depth = previousDepth;
            GUI.color = savedColor;

            // Track if position changed significantly (moved more than 5 pixels)
            Vector2 currentPos = new Vector2(_windowRect.x, _windowRect.y);
            float distance = Vector2.Distance(currentPos, _lastSavedPosition);

            // Save position only if moved significantly and mouse button released (not dragging)
            if (distance > 5f && !Input.GetMouseButton(0))
            {
                Config.UIConfig.WindowX = _windowRect.x;
                Config.UIConfig.WindowY = _windowRect.y;
                Config.UIConfig.SaveWindowPosition(); // Single save for both coordinates
                _lastSavedPosition = currentPos;
                MelonLogger.Msg($"[CooldownTracker] Window position saved: ({_windowRect.x:F0}, {_windowRect.y:F0})");
            }
        }

        private static void InitializeStyles()
        {
            _windowStyle = new GUIStyle(GUI.skin.window)
            {
                padding = new RectOffset(10, 10, 20, 10),
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };

            _headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                normal = { textColor = Color.yellow }
            };

            _stylesInitialized = true;
        }

        private static void DrawCooldownWindow(int windowId)
        {
            // Make window draggable by title bar
            GUI.DragWindow(new Rect(0, 0, WINDOW_WIDTH, 20));

            float currentY = 25f;

            // Draw header
            GUI.Label(new Rect(10, currentY, WINDOW_WIDTH - 20, 20), $"Customers: {_customerCooldowns.Count}", _headerStyle);
            currentY += 25f;

            // Begin scroll view
            Rect scrollViewRect = new Rect(5, currentY, WINDOW_WIDTH - 10, WINDOW_HEIGHT - currentY - 5);
            Rect scrollContentRect = new Rect(0, 0, WINDOW_WIDTH - 30, _customerCooldowns.Count * 25f);

            _scrollPosition = GUI.BeginScrollView(scrollViewRect, _scrollPosition, scrollContentRect);

            // Draw each customer's cooldown
            float itemY = 0f;
            var sortedCooldowns = _customerCooldowns.OrderBy(kvp => kvp.Value).ToList();

            foreach (var kvp in sortedCooldowns)
            {
                float remainingTime = kvp.Value - Time.time;
                if (remainingTime > 0)
                {
                    int seconds = Mathf.CeilToInt(remainingTime);
                    string displayText = $"⏱ {kvp.Key}: {seconds}s";
                    GUI.Label(new Rect(5, itemY, WINDOW_WIDTH - 30, 20), displayText, _labelStyle);
                    itemY += 25f;
                }
            }

            GUI.EndScrollView();
        }

        public static void StartCooldownTracking(string customerName, float remainingSeconds)
        {
            if (remainingSeconds <= 0)
            {
                _customerCooldowns.Remove(customerName);
                MelonLogger.Msg($"[CooldownTracker] Removed {customerName} (expired/invalid)");
                return;
            }

            // Auto-show window when first customer added
            bool wasEmpty = _customerCooldowns.Count == 0;

            float expiryTime = Time.time + remainingSeconds;
            _customerCooldowns[customerName] = expiryTime;

            MelonLogger.Msg($"[CooldownTracker] Added {customerName}: {remainingSeconds:F0}s remaining (total tracked: {_customerCooldowns.Count})");

            // Show window automatically when first cooldown starts
            if (wasEmpty)
            {
                _forceShow = true; // Force immediate visibility
                Config.UIConfig.ShowWindow = true; // Also update config
                _verboseEventLogging = true; // Enable detailed event logging
                _debugEventLogStart = Time.time; // Start verbose logging timer
                MelonLogger.Msg($"[CooldownTracker] Auto-showing window (verbose event logging enabled for 5s)");
            }
        }

        public static void StopCooldownTracking(string customerName)
        {
            _customerCooldowns.Remove(customerName);

            // TODO: Trigger dialog refresh when cooldown expires while dialog is open
            // Current limitation: If you're in the customer dialog when cooldown expires,
            // you need to exit and re-enter the dialog to see the offer become available
            // Future: Could patch the dialog update method to re-check OfferDealValid periodically
        }
#endif
    }
}
