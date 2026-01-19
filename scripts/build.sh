#!/bin/bash
set -e

echo "Building Schedule I Mod..."
dotnet build src/ScheduleIMod.csproj -c Release

echo ""
echo "Build complete!"
echo "  Mono build: build/mono/ScheduleIMod.dll"
echo "  IL2CPP build: build/il2cpp/ScheduleIMod.dll"
