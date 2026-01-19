# Schedule I Mod

A MelonLoader mod for Schedule I that supports both Mono (beta) and IL2CPP (production) builds.

## Features

### Configurable Deal Cooldown

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

### Development Features

- Multi-target build system (Mono + IL2CPP)
- xUnit test project with FluentAssertions
- Testable business logic separation
- Harmony patching examples

## Planned Features

### Enhanced Console Command Interface

Improve the in-game console experience with:

- **Command History**: Navigate recent commands with arrow keys (Up/Down)
  - Persistent history across game sessions
  - Configurable history size

- **Autocomplete Support**: Tab completion for commands and parameters
  - Command name completion
  - Parameter suggestions based on available options
  - Smart context-aware suggestions

## Installation

### For Users
1. Download the latest release
2. Extract to `<Schedule I>/Mods/` folder
3. Launch the game

### For Developers
See [Development Setup](#development-setup)

## Development Setup

### Prerequisites
- .NET 6.0 SDK
- .NET Framework 4.7.2 Developer Pack
- Rider IDE or Visual Studio
- WSL Ubuntu (recommended)
- Schedule I with MelonLoader 0.7.1 installed

### First-Time Setup

1. **Install MelonLoader on Schedule I**:
   - Download MelonLoader 0.7.1 from [Thunderstore](https://thunderstore.io/c/schedule-i/p/LavaGang/MelonLoader/)
   - Run the installer and point it to your game directory
   - Launch the game once to generate unhollowed assemblies

2. **Verify game namespaces** (important!):
   - Switch to beta "alternate" branch in Steam for Mono build
   - Download [dnSpy](https://github.com/dnSpy/dnSpy/releases)
   - Open `<game>/Schedule I_Data/Managed/Assembly-CSharp.dll`
   - Verify the root namespace (guide assumes `ScheduleOne`)
   - Update `using` statements in code if different

3. **Clone and setup**:
   ```bash
   cd ~/projects
   git clone <your-repo>
   cd schedule-i-mod
   chmod +x scripts/*.sh
   ```

4. **Setup game assembly references** (REQUIRED before building):

   **For IL2CPP** (main/production branch):
   ```bash
   ./scripts/setup-refs.sh "<GAME_DIR>"
   ```
   Example: `./scripts/setup-refs.sh "/mnt/c/SteamLibrary/steamapps/common/Schedule I"`

   **For Mono** (beta "alternate" branch):
   - Switch branch in Steam (Properties → Betas → "alternate")
   - Run game once
   - Run setup script again:
   ```bash
   ./scripts/setup-refs.sh "<GAME_DIR>"
   ```

   **Note**: Replace `<GAME_DIR>` with your Schedule I installation path. Common locations:
   - `/mnt/c/Program Files (x86)/Steam/steamapps/common/Schedule I`
   - `/mnt/c/SteamLibrary/steamapps/common/Schedule I`
   - Use `find /mnt -name "Schedule I" -type d 2>/dev/null` to locate it

5. **Build**:
   ```bash
   ./scripts/build.sh
   ```

6. **Deploy and test**:
   ```bash
   # For IL2CPP (default)
   ./scripts/deploy.sh "<GAME_DIR>" il2cpp

   # For Mono
   ./scripts/deploy.sh "<GAME_DIR>" mono
   ```

### Development Workflow

1. Edit code in `src/`
2. Run `./scripts/build.sh`
3. Run `./scripts/deploy.sh "<game_dir>" il2cpp`
4. Launch game and check MelonLoader console
5. Check `<game>/MelonLoader/Latest.log` for detailed output

### Switching Between IL2CPP and Mono

**To test Mono branch**:
1. Steam: Schedule I → Properties → Betas → Select "alternate"
2. Run game once to regenerate assemblies
3. Re-run `./scripts/setup-refs.sh "<game_dir>"`
4. Build and deploy with `mono` parameter

**To return to IL2CPP**:
1. Steam: Schedule I → Properties → Betas → "None"
2. Build and deploy with `il2cpp` parameter (or omit for default)

## Configuration

Settings are stored in: `<Schedule I>/UserData/MelonPreferences.cfg`

**Economy Category Settings:**
- `DealCooldownMultiplier` (float, default: 1.0) - Scales customer deal cooldowns
- `DebugLogLevel` (enum, default: None) - Controls debug logging verbosity

Edit the file manually or use the [Schedule I Mod Manager](https://github.com/Prowiler/schedule1-mod-manager-wiki) for a GUI. Changes apply immediately without restarting the game.

## Known Issues

- Namespaces must be verified with dnSpy before building
- Toast notifications (in-game messages) not yet implemented - currently logs to console as fallback

## Credits

- Based on [S1 Modding Wiki](https://s1modding.github.io/)
- Powered by MelonLoader 0.7.1 and HarmonyX
