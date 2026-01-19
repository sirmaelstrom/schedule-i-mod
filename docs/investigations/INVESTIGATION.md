# Cooldown Tracker Window Visibility Investigation

## Current Status (2026-01-19 02:04 AM)

**Problem**: Cooldown tracker window doesn't appear immediately when auto-show triggers. Only becomes visible after ANY customer dialog interaction.

**Confirmed Facts**:
1. OnGUI executes correctly from the start (Layout + Repaint events confirmed in logs)
2. GUI.Box rendering code executes (logs confirm)
3. Window state: `_forceShow=true`, `Config.UIConfig.ShowWindow=true`, `_customerCooldowns.Count > 0`
4. All early exit conditions pass - code reaches the rendering section
5. Window uses `GUI.Box` at position (3711, 540) - RIGHT side of 5120x1440 ultrawide monitor
6. Window appears with blue title bar and dark gray background - BUT ONLY AFTER customer interaction

## Critical Clue (Session End Finding)

**Red test box that DID appear immediately**:
- Used hardcoded position on LEFT side: `new Rect(100, 100, 200, 100)`
- Used `GUI.color = Color.red`
- Appeared IMMEDIATELY when triggered

**Current window that doesn't appear immediately**:
- Uses `_windowRect` at position (3711, 540) from config - RIGHT side of screen
- Uses `GUI.color` for dark gray/blue colors
- Only appears AFTER customer interaction

**Hypothesis**: Position-related rendering issue on ultrawide monitor. Window may be rendering in an area that's not immediately visible or requires interaction to become visible.

## What We've Tried (All Failed)

1. ❌ GUI.Window → GUI.Box conversion
2. ❌ GUI.BringWindowToFront()
3. ❌ GUI.FocusWindow()
4. ❌ GUI.depth = -1000
5. ❌ Removing all GUIStyles (to match simple test box)
6. ❌ Adding explicit colors (dark gray/blue - window is visible but still delayed)

## Next Steps to Try

### HIGH PRIORITY: Test Position Hypothesis
1. **Temporarily hardcode position to LEFT side** (100, 100) - exactly where red test box was
2. If this makes it appear immediately, then issue is position-related
3. Investigate why position (3711, 540) requires interaction to become visible

### Implementation:
```csharp
// In OnSceneWasLoaded or OnGUI initialization:
// TEMPORARILY override config position for testing
_windowRect = new Rect(100, 100, WINDOW_WIDTH, WINDOW_HEIGHT);
```

### If Position Fix Works:
- Determine safe initial position for ultrawide monitors
- Add position validation (ensure window spawns in visible area)
- Consider centering window on first show instead of using saved position

## Code State

**Current Implementation**:
- File: `src/ModMain.cs`
- Using: `GUI.Box` with explicit colors (dark gray background, blue title bar)
- Position: From config (3711, 540)
- Auto-show: Works but with visibility delay

**Recent Changes**:
- Added explicit `GUI.color` settings to make window obvious
- Window is now visually distinct (blue title bar, dark gray background)
- Still exhibits delayed visibility issue

## Resume Instructions

1. Test hardcoded left-side position (100, 100)
2. If that fixes immediate visibility, implement position validation
3. If that doesn't fix it, consider alternative UI approaches (Unity UI vs IMGUI)

## Environment
- Game: Schedule I (IL2CPP build)
- Monitor: 5120x1440 ultrawide
- Framework: MelonLoader 0.7.1
- UI System: Unity IMGUI (OnGUI)
