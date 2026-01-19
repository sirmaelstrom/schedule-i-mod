# Schedule I Game Code Analysis

Generated from decompiled Assembly-CSharp.dll (Mono branch)

## Namespace Verification

✓ Root namespace is **`ScheduleOne`** (matches our code)
✓ No changes needed to existing `using` statements

## Key Classes for Modding

### 1. Economy & Customers

**Location:** `ScheduleOne.Economy.Customer`

**Method:** `OfferDealValid(out string invalidReason)` (Line 1678)
- Returns `bool` - whether customer can receive deal offer
- Has cooldown timer (360 seconds = 6 minutes)
- Checks for pending offers
- **Mod opportunity:** Bypass cooldowns, force deals always valid

**Code:**
```csharp
protected virtual bool OfferDealValid(out string invalidReason)
{
    invalidReason = string.Empty;
    if (this.TimeSinceLastDealCompleted < 360)  // 6 minute cooldown
    {
        invalidReason = "Customer recently completed a deal";
        return false;
    }
    if (this.OfferedContractInfo != null)
    {
        invalidReason = "Customer already has a pending offer";
        return false;
    }
    if (this.TimeSinceInstantDealOffered < 360 && !this.pendingInstantDeal)
    {
        invalidReason = "Already recently offered";
        return false;
    }
    return true;
}
```

### 2. Money System

**Location:** `ScheduleOne.Money.MoneyManager`

**Method:** `ChangeCashBalance(float change, bool visualizeChange = true, bool playCashSound = false)` (Line 287)
- Modifies player cash
- `change` can be positive (add) or negative (subtract)
- **Mod opportunity:** Intercept transactions, multiply income, prevent expenses

### 3. Save/Load System

**Location:** `ScheduleOne.Persistence.SaveManager`

**Method:** `Save()` (Line 198)
- Triggers game save
- Has validation checks (IsServer, IsSaving, IsTutorial)
- **Mod opportunity:** Auto-save triggers, save notifications, backup hooks

**Code:**
```csharp
public void Save()
{
    this.Save(Singleton<LoadManager>.Instance.LoadedGameFolderPath);
}

public void Save(string saveFolderPath)
{
    // Validates server, loaded game, not already saving, not tutorial
    // Then executes save to specified folder
}
```

## Other Interesting Namespaces

- **`ScheduleOne.Product`** - Drug products, quality, effects
- **`ScheduleOne.Law`** - Police, arrests, wanted level
- **`ScheduleOne.NPCs`** - NPC behavior, AI, relationships
- **`ScheduleOne.Levelling`** - XP and progression
- **`ScheduleOne.PlayerScripts`** - Player character, inventory, movement
- **`ScheduleOne.Building`** - Property management, upgrades
- **`ScheduleOne.Combat`** - Weapons, damage, health
- **`ScheduleOne.Cartel`** - Gang/cartel system
- **`ScheduleOne.Police`** - Law enforcement mechanics

## Recommended First Patches

### Easy Difficulty
1. **No Deal Cooldown** - Prefix patch on `Customer.OfferDealValid` to always return true
2. **Save Notification** - Postfix patch on `SaveManager.Save` to log when saves occur
3. **Money Multiplier** - Prefix patch on `MoneyManager.ChangeCashBalance` to multiply positive amounts

### Medium Difficulty
4. **Custom Customer AI** - Patch customer behavior methods
5. **Modified Product Quality** - Adjust product stats/effects
6. **XP Boost** - Modify levelling calculations

### Advanced
7. **Police Evade** - Modify law enforcement detection
8. **Custom Game Events** - Hook into game manager lifecycle
9. **Network Modifications** - Multiplayer behavior changes (requires NetworkBehaviour understanding)

## Example Patch Implementations

See example files in `src/Patches/`:
- `ExamplePrefix.cs` - Prefix pattern (modify before)
- `ExamplePostfix.cs` - Postfix pattern (modify after)
- `ExampleIL2CPP.cs` - IL2CPP-specific techniques

To enable examples:
1. Change `#if FALSE` to `#if TRUE` in patch files
2. Verify class/method names match game code
3. Build and test
