#!/bin/bash
# Usage: ./deploy.sh [game_directory] [mono|il2cpp]
# Example: ./deploy.sh "/mnt/c/SteamLibrary/steamapps/common/Schedule I" il2cpp
#
# If game_directory is omitted, uses SCHEDULE_I_DIR environment variable
# Set in ~/.bashrc: export SCHEDULE_I_DIR="/path/to/Schedule I"

set -e

GAME_DIR="${1:-$SCHEDULE_I_DIR}"
BUILD_TYPE="${2:-il2cpp}" # Default to il2cpp

if [ -z "$GAME_DIR" ]; then
    echo "Error: No game directory specified"
    echo "Usage: $0 [game_directory] [mono|il2cpp]"
    echo "Example: $0 \"/mnt/c/SteamLibrary/steamapps/common/Schedule I\" il2cpp"
    echo ""
    echo "Or set SCHEDULE_I_DIR environment variable:"
    echo "  export SCHEDULE_I_DIR=\"/path/to/Schedule I\""
    exit 1
fi

if [ ! -d "$GAME_DIR" ]; then
    echo "Error: Game directory not found: $GAME_DIR"
    exit 1
fi

if [ ! -f "build/$BUILD_TYPE/ScheduleIMod.dll" ]; then
    echo "Error: Build not found at build/$BUILD_TYPE/ScheduleIMod.dll"
    echo "Run ./build.sh first"
    exit 1
fi

# Check if game is running (Windows process check from WSL)
GAME_EXE="Schedule I.exe"
if /mnt/c/Windows/System32/tasklist.exe 2>/dev/null | grep -q "$GAME_EXE"; then
    echo "⚠ WARNING: Game appears to be running!"
    echo "   Close the game before deploying to avoid file lock issues."
    echo ""
    read -p "Continue anyway? (y/N) " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        echo "Deploy cancelled"
        exit 1
    fi
fi

MODS_DIR="$GAME_DIR/Mods"
mkdir -p "$MODS_DIR"

echo "Deploying $BUILD_TYPE build to: $MODS_DIR"
cp "build/$BUILD_TYPE/ScheduleIMod.dll" "$MODS_DIR/"

echo ""
echo "Deploy complete! Launch the game to test."
echo "Check MelonLoader console or Latest.log for mod output."
