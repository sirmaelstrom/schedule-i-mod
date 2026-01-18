# Configurable Deal Cooldown Feature Design

**Date:** 2026-01-18
**Feature:** Configurable customer deal cooldown with MelonLoader preferences integration
**Status:** Design approved, ready for implementation

## Overview

First functional feature for ScheduleIMod that demonstrates:
- Harmony patching with game logic modification
- MelonLoader preferences integration
- Mod Manager compatibility
- Hot reload configuration changes
- Testable business logic separation
- Configurable debug logging

## Goals

1. **Functional** - Allow users to adjust customer deal cooldown timing
2. **Educational** - Demonstrate MelonLoader preferences, Harmony patterns, testability
3. **User-friendly** - Hot reload, mod manager integration, helpful debug output
4. **Maintainable** - Tested business logic, clean separation of concerns

## Architecture

### File Structure

```
src/
├── Config/
│   └── EconomyConfig.cs           # MelonLoader preferences wrapper
├── Logic/
│   └── CooldownLogic.cs           # Testable business logic
├── Patches/
│   └── CustomerDealCooldownPatch.cs # Harmony prefix patch
├── Utils/
│   └── DebugLogger.cs             # Centralized logging with level filtering
└── ModMain.cs                     # Initialize config in OnInitializeMelon

tests/
└── ScheduleIMod.Tests/
    ├── ScheduleIMod.Tests.csproj  # xUnit test project (net6.0)
    └── Logic/
        └── CooldownLogicTests.cs  # Unit tests for cooldown calculations
```

### Configuration System

**MelonLoader Preferences Category:** "Economy"

**Settings:**
1. **DealCooldownMultiplier** (float)
   - Range: 0.0 - 2.0
   - Default: 1.0 (vanilla behavior)
   - Step: 0.1
   - Description: "Scales customer deal cooldowns (0.0 = instant, 1.0 = vanilla 360s, 2.0 = double)"
   - Calculated display: "Current cooldown: XXXs"

2. **DebugLogLevel** (enum dropdown)
   - Options: None, Console, Toast
   - Default: None
   - Description: "None = silent, Console = log to console, Toast = console + in-game messages"

**Hot Reload:**
- Settings saved to `UserData/MelonPreferences.cfg`
- `OnEntryValueChanged` callbacks fire immediately when mod manager changes values
- Patch reads `EconomyConfig.CooldownMultiplier` on every check (no caching)
- No game restart required

### Business Logic Separation

**Testable Logic** (`CooldownLogic.cs`):
- Pure static functions with no Unity/MelonLoader dependencies
- `ScaleCooldown(baseSeconds, multiplier)` - Math calculation
- `ShouldAllowDeal(...)` - Complete validation with all game state inputs
- Returns bool + invalidReason string
- All edge cases testable in isolation

**Harmony Patch** (`CustomerDealCooldownPatch.cs`):
- Thin wrapper over `CooldownLogic`
- Extracts game state from `Customer` instance
- Calls testable logic
- Sets `__result` and `invalidReason` out parameter
- Error handling with fallback to vanilla behavior
- Debug logging with current settings

### Patch Behavior

**Target Method:** `Customer.OfferDealValid(out string invalidReason)`

**Patch Type:** Prefix (skip original method)

**Modified Checks:**
1. **Pending offer check** - UNCHANGED, always respected (prevents game state issues)
2. **TimeSinceLastDealCompleted** - Scaled by multiplier (360 * multiplier)
3. **TimeSinceInstantDealOffered** - Scaled by multiplier (360 * multiplier)

**Example Behavior:**
- Multiplier 0.0: Cooldowns effectively removed (0 < any positive time)
- Multiplier 0.5: 180 second cooldown (half of vanilla 360s)
- Multiplier 1.0: Vanilla behavior (360 seconds)
- Multiplier 2.0: Doubled cooldown (720 seconds, harder mode)

**Safety:**
- Pending offer check prevents offering to customers who already have deals
- Try-catch with fallback to vanilla on any error
- No modification of game state, only return value

### Debug Logging

**DebugLogger Utility:**
- Respects `EconomyConfig.LogLevel`
- **None**: No logging, zero overhead
- **Console**: Logs to MelonLoader console only
- **Toast**: Console + in-game toast notifications (research game's notification system)

**Logged Events:**
- Configuration changes: "Deal cooldown changed: 1.0x -> 0.5x (180s)"
- Deal checks: "Deal check: allowed=true, reason=''"
- Toast fallback: Gracefully skip if game notification system unavailable

### Testing Strategy

**Test Project:**
- Framework: xUnit
- Target: net6.0 only (tests don't need multi-target)
- Additional packages: FluentAssertions for readable assertions
- Coverage tool: ReSharper coverage analysis

**Test Coverage Goals:**

**What to test (meaningful coverage):**
1. `CooldownLogic.ScaleCooldown()` - Edge cases: 0.0, 0.5, 1.0, 2.0 multipliers
2. `CooldownLogic.ShouldAllowDeal()` - Boundary conditions:
   - Pending offer check (always blocks, even with 0.0 multiplier)
   - Time exactly at cooldown boundary (359s vs 360s)
   - Scaled cooldown boundaries (179s vs 180s for 0.5x)
   - Zero multiplier behavior
3. Configuration validation - Range limits, defaults

**What NOT to test (no meaningful value):**
- Harmony patching mechanics (requires game runtime)
- Unity integration
- MelonLoader preferences system
- Actual in-game behavior end-to-end

**Example Test Cases:**
```csharp
[Theory]
[InlineData(0.0f, 0f)]     // No cooldown
[InlineData(0.5f, 180f)]   // Half cooldown
[InlineData(1.0f, 360f)]   // Vanilla
[InlineData(2.0f, 720f)]   // Double
public void ScaleCooldown_CalculatesCorrectly(float multiplier, float expected)

[Fact]
public void ShouldAllowDeal_BlocksWhenPendingOfferExists()
// Even with 0.0 multiplier, pending offer blocks

[Theory]
[InlineData(0.5f, 179f, false)]  // Just under boundary
[InlineData(0.5f, 180f, true)]   // At boundary
public void ShouldAllowDeal_RespectsScaledCooldown(...)
```

## Implementation Plan

1. **Setup test project** - Add ScheduleIMod.Tests.csproj to solution
2. **Implement CooldownLogic** with tests (TDD approach)
3. **Create EconomyConfig** with MelonLoader preferences
4. **Create DebugLogger** utility
5. **Implement CustomerDealCooldownPatch** (thin wrapper)
6. **Initialize in ModMain.OnInitializeMelon()**
7. **Build and test** - Verify hot reload in mod manager
8. **Research toast notifications** - Find game's notification system for Toast mode
9. **Document usage** - Update README with feature description

## Success Criteria

- ✓ Settings appear in Mod Manager under "Economy" category
- ✓ Multiplier slider (0.0-2.0) changes cooldown in real-time
- ✓ Calculated cooldown seconds displayed for user reference
- ✓ Debug logging works at all three levels (None/Console/Toast)
- ✓ Hot reload works - no restart needed for setting changes
- ✓ Pending offer check still prevents duplicate offers
- ✓ Tests pass with meaningful coverage (>80% of CooldownLogic)
- ✓ No errors in MelonLoader console
- ✓ Multiplier 0.0 allows instant deals
- ✓ Multiplier 1.0 preserves vanilla behavior exactly

## Future Enhancements

- Additional economy settings in same category (money multipliers, XP boosts)
- Per-customer cooldown overrides
- Cooldown notification UI overlay
- Statistics tracking (deals completed, average cooldown used)
- Preset configurations (Easy/Normal/Hard modes)

## Technical Notes

**Multi-target Considerations:**
- Logic is runtime-agnostic (works on both Mono and IL2CPP)
- No `#if IL2CPP` conditionals needed for this feature
- Test project targets net6.0 only (simpler, sufficient)

**Mod Manager Integration:**
- Automatically reads `MelonPreferences.cfg`
- No additional integration code needed
- Settings UI generated from MelonLoader metadata

**Performance:**
- Minimal overhead: single multiplication per deal check
- No caching needed - config reads are cheap
- Debug logging only fires when enabled
