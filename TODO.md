# Schedule I Mod - Tasks

## Active

- [ ] Fix namespace issues in src/ModMain.cs (if any)
  - [ ] Verify game namespaces with dnSpy
  - [ ] Update using statements in all .cs files
  - [ ] Test build after namespace fixes
- [ ] Implement configurable deal cooldown feature
  - [ ] Design: See `docs/plans/2026-01-18-configurable-deal-cooldown-design.md`
  - [ ] Implementation: See `docs/plans/2026-01-18-configurable-deal-cooldown-implementation.md`


## Ideas / Future

- [ ] Add config UI in in-game menu
- [ ] Implement save/load for mod settings
- [ ] Add logging for debugging dealer interactions
- [ ] Create installer script for easier deployment
- [ ] Add unit tests for core mod logic

## Completed

- [x] Setup project structure with dual Mono/IL2CPP builds
- [x] Configure build scripts (build.sh, deploy.sh, setup-refs.sh)
- [x] Create documentation (README.md, Development.md, Game-Analysis.md)
- [x] Design configurable deal cooldown system
- [x] Plan implementation approach
- [x] Document game directory in local.config.md (E:\SteamLibrary\steamapps\common\Schedule I)
- [x] Verify MelonLoader installed (confirmed at game directory)
- [x] Verify unhollowed assemblies generated (Il2CppAssemblies folder present)
- [x] Run setup-refs.sh to copy game assemblies (libs/il2cpp/ populated)
- [x] Verify project builds successfully (both Mono and IL2CPP targets build with 0 errors)
- [x] Deploy to game directory (2026-01-19)
- [x] Test mod loads in-game successfully (2026-01-19)
