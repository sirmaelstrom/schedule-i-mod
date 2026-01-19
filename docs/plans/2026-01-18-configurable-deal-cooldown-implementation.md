# Configurable Deal Cooldown Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Implement configurable customer deal cooldown with MelonLoader preferences, testable business logic, and hot reload support.

**Architecture:** Test-first approach with pure business logic (CooldownLogic), thin Harmony patch wrapper, MelonLoader preferences for hot reload, configurable debug logging.

**Tech Stack:** C# (.NET 6.0 + .NET Framework 4.7.2), xUnit, FluentAssertions, MelonLoader 0.7.1, HarmonyX

---

## Task 1: Create Test Project

**Files:**
- Create: `tests/ScheduleIMod.Tests/ScheduleIMod.Tests.csproj`
- Modify: `ScheduleIMod.sln`

**Step 1: Create tests directory**

```bash
mkdir -p tests/ScheduleIMod.Tests/Logic
```

**Step 2: Create test project file**

Create file: `tests/ScheduleIMod.Tests/ScheduleIMod.Tests.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="xunit" Version="2.4.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.4.5">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.6.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\ScheduleIMod.csproj" />
  </ItemGroup>
</Project>
```

**Step 3: Add project to solution**

Run: `dotnet sln add tests/ScheduleIMod.Tests/ScheduleIMod.Tests.csproj`
Expected: "Project `tests/ScheduleIMod.Tests/ScheduleIMod.Tests.csproj` added to the solution."

**Step 4: Verify project added**

Run: `dotnet sln list`
Expected: Both src/ScheduleIMod.csproj and tests/ScheduleIMod.Tests/ScheduleIMod.Tests.csproj listed

**Step 5: Restore and verify test project builds**

Run: `dotnet build tests/ScheduleIMod.Tests/ScheduleIMod.Tests.csproj`
Expected: Build succeeded

**Step 6: Commit**

```bash
git add tests/ ScheduleIMod.sln
git commit -m "test: add xUnit test project with FluentAssertions"
```

---

## Task 2: Implement CooldownLogic.ScaleCooldown (TDD)

**Files:**
- Create: `tests/ScheduleIMod.Tests/Logic/CooldownLogicTests.cs`
- Create: `src/Logic/CooldownLogic.cs`

**Step 1: Write failing test for ScaleCooldown**

Create file: `tests/ScheduleIMod.Tests/Logic/CooldownLogicTests.cs`

```csharp
using FluentAssertions;
using ScheduleIMod.Logic;
using Xunit;

namespace ScheduleIMod.Tests.Logic
{
    public class CooldownLogicTests
    {
        [Theory]
        [InlineData(0.0f, 0f)]     // No cooldown
        [InlineData(0.5f, 180f)]   // Half cooldown
        [InlineData(1.0f, 360f)]   // Vanilla
        [InlineData(2.0f, 720f)]   // Double
        public void ScaleCooldown_CalculatesCorrectly(float multiplier, float expected)
        {
            var result = CooldownLogic.ScaleCooldown(360f, multiplier);
            result.Should().Be(expected);
        }
    }
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/ScheduleIMod.Tests/ScheduleIMod.Tests.csproj`
Expected: FAIL - "The type or namespace name 'CooldownLogic' could not be found"

**Step 3: Create Logic directory**

Run: `mkdir -p src/Logic`

**Step 4: Create minimal CooldownLogic implementation**

Create file: `src/Logic/CooldownLogic.cs`

```csharp
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
```

**Step 5: Run test to verify it passes**

Run: `dotnet test tests/ScheduleIMod.Tests/ScheduleIMod.Tests.csproj --filter "ScaleCooldown_CalculatesCorrectly"`
Expected: PASS - 4 tests passed

**Step 6: Commit**

```bash
git add src/Logic/CooldownLogic.cs tests/ScheduleIMod.Tests/Logic/CooldownLogicTests.cs
git commit -m "feat: add CooldownLogic.ScaleCooldown with tests"
```

---

## Task 3: Implement CooldownLogic.ShouldAllowDeal - Pending Offer Check (TDD)

**Files:**
- Modify: `tests/ScheduleIMod.Tests/Logic/CooldownLogicTests.cs`
- Modify: `src/Logic/CooldownLogic.cs`

**Step 1: Write failing test for pending offer block**

Add to `tests/ScheduleIMod.Tests/Logic/CooldownLogicTests.cs`:

```csharp
[Fact]
public void ShouldAllowDeal_BlocksWhenPendingOfferExists()
{
    var result = CooldownLogic.ShouldAllowDeal(
        timeSinceLastCompleted: 1000f,
        timeSinceLastOffered: 1000f,
        hasPendingOffer: true,
        hasPendingInstantDeal: false,
        cooldownMultiplier: 0.0f,
        out string invalidReason
    );

    result.Should().BeFalse();
    invalidReason.Should().Contain("pending offer");
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/ScheduleIMod.Tests/ScheduleIMod.Tests.csproj --filter "ShouldAllowDeal_BlocksWhenPendingOfferExists"`
Expected: FAIL - "Method 'ShouldAllowDeal' not found"

**Step 3: Implement ShouldAllowDeal with pending offer check**

Add to `src/Logic/CooldownLogic.cs`:

```csharp
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
```

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/ScheduleIMod.Tests/ScheduleIMod.Tests.csproj --filter "ShouldAllowDeal_BlocksWhenPendingOfferExists"`
Expected: PASS

**Step 5: Commit**

```bash
git add src/Logic/CooldownLogic.cs tests/ScheduleIMod.Tests/Logic/CooldownLogicTests.cs
git commit -m "feat: add pending offer check to ShouldAllowDeal"
```

---

## Task 4: Implement CooldownLogic.ShouldAllowDeal - Cooldown Checks (TDD)

**Files:**
- Modify: `tests/ScheduleIMod.Tests/Logic/CooldownLogicTests.cs`
- Modify: `src/Logic/CooldownLogic.cs`

**Step 1: Write failing tests for cooldown boundaries**

Add to `tests/ScheduleIMod.Tests/Logic/CooldownLogicTests.cs`:

```csharp
[Theory]
[InlineData(0.0f, 100f, true)]   // 0x multiplier, any time = pass
[InlineData(1.0f, 359f, false)]  // 1x, just under 360s = fail
[InlineData(1.0f, 360f, true)]   // 1x, exactly at 360s = pass
[InlineData(1.0f, 361f, true)]   // 1x, over 360s = pass
[InlineData(0.5f, 179f, false)]  // 0.5x, under 180s = fail
[InlineData(0.5f, 180f, true)]   // 0.5x, at 180s = pass
[InlineData(2.0f, 719f, false)]  // 2.0x, under 720s = fail
[InlineData(2.0f, 720f, true)]   // 2.0x, at 720s = pass
public void ShouldAllowDeal_RespectsScaledCooldown_LastCompleted(
    float multiplier,
    float timeSince,
    bool shouldAllow)
{
    var result = CooldownLogic.ShouldAllowDeal(
        timeSinceLastCompleted: timeSince,
        timeSinceLastOffered: 1000f, // Well past cooldown
        hasPendingOffer: false,
        hasPendingInstantDeal: false,
        cooldownMultiplier: multiplier,
        out _
    );

    result.Should().Be(shouldAllow);
}

[Theory]
[InlineData(0.5f, 179f, false)]  // Just under boundary
[InlineData(0.5f, 180f, true)]   // At boundary
public void ShouldAllowDeal_RespectsScaledCooldown_LastOffered(
    float multiplier,
    float timeSince,
    bool shouldAllow)
{
    var result = CooldownLogic.ShouldAllowDeal(
        timeSinceLastCompleted: 1000f, // Well past cooldown
        timeSinceLastOffered: timeSince,
        hasPendingOffer: false,
        hasPendingInstantDeal: false,
        cooldownMultiplier: multiplier,
        out _
    );

    result.Should().Be(shouldAllow);
}
```

**Step 2: Run tests to verify they fail**

Run: `dotnet test tests/ScheduleIMod.Tests/ScheduleIMod.Tests.csproj --filter "ShouldAllowDeal_RespectsScaledCooldown"`
Expected: FAIL - Some tests pass (wrong behavior), some fail

**Step 3: Implement complete cooldown logic**

Replace `ShouldAllowDeal` in `src/Logic/CooldownLogic.cs`:

```csharp
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

    // Scale the cooldown checks
    float scaledCooldown = ScaleCooldown(360f, cooldownMultiplier);

    if (timeSinceLastCompleted < scaledCooldown)
    {
        invalidReason = $"Customer recently completed a deal (cooldown: {scaledCooldown:F0}s)";
        return false;
    }

    if (timeSinceLastOffered < scaledCooldown && !hasPendingInstantDeal)
    {
        invalidReason = $"Already recently offered (cooldown: {scaledCooldown:F0}s)";
        return false;
    }

    return true;
}
```

**Step 4: Run tests to verify they pass**

Run: `dotnet test tests/ScheduleIMod.Tests/ScheduleIMod.Tests.csproj --filter "CooldownLogicTests"`
Expected: PASS - All tests pass

**Step 5: Commit**

```bash
git add src/Logic/CooldownLogic.cs tests/ScheduleIMod.Tests/Logic/CooldownLogicTests.cs
git commit -m "feat: implement scaled cooldown checks in ShouldAllowDeal"
```

---

## Task 5: Create EconomyConfig with MelonLoader Preferences

**Files:**
- Create: `src/Config/EconomyConfig.cs`

**Step 1: Create Config directory**

Run: `mkdir -p src/Config`

**Step 2: Create EconomyConfig class**

Create file: `src/Config/EconomyConfig.cs`

```csharp
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
```

**Step 3: Verify file compiles**

Run: `dotnet build src/ScheduleIMod.csproj`
Expected: Build fails - "Utils.DebugLogger does not exist" (expected, we'll create it next)

**Step 4: Comment out DebugLogger calls temporarily**

In `src/Config/EconomyConfig.cs`, comment out the DebugLogger.Log calls:

```csharp
private static void OnCooldownMultiplierChanged(float oldValue, float newValue)
{
    // TODO: Uncomment after DebugLogger is implemented
    // Utils.DebugLogger.Log($"Deal cooldown changed: {oldValue:F1}x -> {newValue:F1}x ({CurrentCooldownSeconds:F0}s)");
}

private static void OnDebugLogLevelChanged(DebugLogLevel oldValue, DebugLogLevel newValue)
{
    // TODO: Uncomment after DebugLogger is implemented
    // Utils.DebugLogger.Log($"Debug log level changed: {oldValue} -> {newValue}");
}
```

**Step 5: Verify build succeeds**

Run: `dotnet build src/ScheduleIMod.csproj`
Expected: Build succeeded

**Step 6: Commit**

```bash
git add src/Config/EconomyConfig.cs
git commit -m "feat: add EconomyConfig with MelonLoader preferences and hot reload"
```

---

## Task 6: Create DebugLogger Utility

**Files:**
- Create: `src/Utils/DebugLogger.cs`
- Modify: `src/Config/EconomyConfig.cs`

**Step 1: Create Utils directory (if needed)**

Run: `mkdir -p src/Utils`

**Step 2: Create DebugLogger class**

Create file: `src/Utils/DebugLogger.cs`

```csharp
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
```

**Step 3: Uncomment DebugLogger calls in EconomyConfig**

In `src/Config/EconomyConfig.cs`, uncomment the DebugLogger.Log calls:

```csharp
private static void OnCooldownMultiplierChanged(float oldValue, float newValue)
{
    Utils.DebugLogger.Log($"Deal cooldown changed: {oldValue:F1}x -> {newValue:F1}x ({CurrentCooldownSeconds:F0}s)");
}

private static void OnDebugLogLevelChanged(DebugLogLevel oldValue, DebugLogLevel newValue)
{
    Utils.DebugLogger.Log($"Debug log level changed: {oldValue} -> {newValue}");
}
```

**Step 4: Verify build succeeds**

Run: `dotnet build src/ScheduleIMod.csproj`
Expected: Build succeeded

**Step 5: Commit**

```bash
git add src/Utils/DebugLogger.cs src/Config/EconomyConfig.cs
git commit -m "feat: add DebugLogger with configurable log levels"
```

---

## Task 7: Implement CustomerDealCooldownPatch

**Files:**
- Create: `src/Patches/CustomerDealCooldownPatch.cs`

**Step 1: Create CustomerDealCooldownPatch class**

Create file: `src/Patches/CustomerDealCooldownPatch.cs`

```csharp
using HarmonyLib;
using MelonLoader;
using System;

#if IL2CPP
using Il2CppScheduleOne.Economy;
#else
using ScheduleOne.Economy;
#endif

namespace ScheduleIMod.Patches
{
    [HarmonyPatch(typeof(Customer), nameof(Customer.OfferDealValid))]
    public static class CustomerDealCooldownPatch
    {
        static bool Prefix(Customer __instance, ref string invalidReason, ref bool __result)
        {
            try
            {
                __result = Logic.CooldownLogic.ShouldAllowDeal(
                    __instance.TimeSinceLastDealCompleted,
                    __instance.TimeSinceInstantDealOffered,
                    __instance.OfferedContractInfo != null,
                    __instance.pendingInstantDeal,
                    Config.EconomyConfig.CooldownMultiplier,
                    out invalidReason
                );

                Utils.DebugLogger.Log($"Deal check: allowed={__result}, reason='{invalidReason}'");

                return false; // Skip original method
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"CustomerDealCooldownPatch error: {ex}");
                return true; // Run original on error
            }
        }
    }
}
```

**Step 2: Verify build succeeds**

Run: `dotnet build src/ScheduleIMod.csproj`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add src/Patches/CustomerDealCooldownPatch.cs
git commit -m "feat: add CustomerDealCooldownPatch with error handling"
```

---

## Task 8: Initialize EconomyConfig in ModMain

**Files:**
- Modify: `src/ModMain.cs`

**Step 1: Add EconomyConfig initialization**

In `src/ModMain.cs`, modify the `OnInitializeMelon` method to add config initialization:

```csharp
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
}
```

**Step 2: Verify build succeeds**

Run: `dotnet build src/ScheduleIMod.csproj`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add src/ModMain.cs
git commit -m "feat: initialize EconomyConfig in ModMain"
```

---

## Task 9: Remove Disabled Example Patches

**Files:**
- Delete: `src/Patches/ExamplePrefix.cs`
- Delete: `src/Patches/ExamplePostfix.cs`
- Delete: `src/Patches/ExampleIL2CPP.cs`

**Step 1: Remove example patch files**

Run: `git rm src/Patches/ExamplePrefix.cs src/Patches/ExamplePostfix.cs src/Patches/ExampleIL2CPP.cs`

**Step 2: Verify build succeeds**

Run: `dotnet build src/ScheduleIMod.csproj`
Expected: Build succeeded

**Step 3: Commit**

```bash
git commit -m "chore: remove disabled example patches"
```

---

## Task 10: Build and Verify Tests Pass

**Files:**
- None (verification step)

**Step 1: Run all tests**

Run: `dotnet test`
Expected: All tests pass

**Step 2: Build both targets**

Run: `dotnet build src/ScheduleIMod.csproj -c Release`
Expected: Build succeeded for both net472 and net6.0

**Step 3: Verify output files**

Run: `ls -lh build/mono/ScheduleIMod.dll build/il2cpp/ScheduleIMod.dll`
Expected: Both DLLs exist with reasonable size (>5KB)

**Step 4: Check test coverage**

Run: `dotnet test --collect:"XPlat Code Coverage"`
Expected: Tests pass, coverage report generated

**Step 5: Commit if any changes**

```bash
git add -A
git commit -m "build: verify tests pass and both targets build successfully"
```

---

## Task 11: Update README with Feature Documentation

**Files:**
- Modify: `README.md`

**Step 1: Add feature section to README**

Add after the "Features" section in `README.md`:

```markdown
## Configurable Features

### Deal Cooldown Control

Adjust the cooldown timer between customer deals:

- **Cooldown Multiplier**: Scale deal cooldowns from instant (0.0x) to double (2.0x)
  - 0.0 = Instant deals (no cooldown)
  - 0.5 = Half cooldown (180 seconds)
  - 1.0 = Vanilla behavior (360 seconds)
  - 2.0 = Harder mode (720 seconds)

- **Debug Logging**: Choose logging verbosity
  - None = Silent operation
  - Console = MelonLoader console logs only
  - Toast = Console + in-game notifications

**Hot Reload**: Settings changes apply immediately without restarting the game.

**Mod Manager**: Configure settings through the [Schedule I Mod Manager](https://github.com/Prowiler/schedule1-mod-manager-wiki) under the "Economy" category.
```

**Step 2: Verify README renders correctly**

Run: `cat README.md | grep -A 20 "Configurable Features"`
Expected: Section displays properly

**Step 3: Commit**

```bash
git add README.md
git commit -m "docs: add configurable deal cooldown feature to README"
```

---

## Task 12: Final Testing Checklist

**Files:**
- None (verification step)

**Step 1: Clean build verification**

Run: `rm -rf build/ src/obj/ src/bin/ tests/ScheduleIMod.Tests/obj/ tests/ScheduleIMod.Tests/bin/ && dotnet build -c Release`
Expected: Build succeeded

**Step 2: Run all tests with verbose output**

Run: `dotnet test --logger "console;verbosity=detailed"`
Expected: All tests pass, detailed output shows each test

**Step 3: Verify multi-target build**

Run: `ls -lh build/mono/ScheduleIMod.dll build/il2cpp/ScheduleIMod.dll`
Expected: Both files exist and are different sizes (mono ~6KB, il2cpp ~7.5KB)

**Step 4: Check for compilation warnings**

Run: `dotnet build src/ScheduleIMod.csproj 2>&1 | grep -i warning`
Expected: No warnings (or only acceptable warnings)

**Step 5: Create summary commit**

```bash
git add -A
git commit -m "test: verify clean build and all tests passing"
```

---

## Success Criteria Verification

After completing all tasks, verify:

- ✓ Test project created with xUnit and FluentAssertions
- ✓ CooldownLogic implemented with >80% test coverage
- ✓ EconomyConfig with MelonLoader preferences (Economy category)
- ✓ DebugLogger with three log levels
- ✓ CustomerDealCooldownPatch as thin wrapper
- ✓ ModMain initializes configuration
- ✓ All tests pass
- ✓ Both Mono and IL2CPP builds succeed
- ✓ README documentation updated
- ✓ Example patches removed
- ✓ No build warnings or errors

## Next Steps (Manual Testing Required)

1. **Deploy to game**: Run `./scripts/deploy.sh "<game_dir>" mono` from main repo
2. **Launch game**: Verify mod loads without errors
3. **Check Mod Manager**: Verify "Economy" category appears with settings
4. **Test hot reload**: Change multiplier slider, verify instant application
5. **Test debug logging**: Enable Console mode, verify logs appear
6. **Test gameplay**: Verify cooldown behavior matches multiplier setting
7. **Test edge cases**: Try 0.0 (instant), 1.0 (vanilla), 2.0 (double)

## Implementation Notes

**TDD Approach:**
- All testable logic (CooldownLogic) developed test-first
- Thin wrappers (Harmony patch, config) created after tests pass
- Continuous verification at each step

**Multi-target Compatibility:**
- Logic is runtime-agnostic (no `#if` directives needed)
- Patch uses conditional compilation for game namespaces
- Test project targets net6.0 only (sufficient)

**Hot Reload:**
- Settings changes trigger callbacks immediately
- Patch reads config on every invocation (no caching)
- No game restart required for setting changes
