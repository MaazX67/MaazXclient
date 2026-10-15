# MaazXClient Universal MultiVer

A Fabric client project with separate Minecraft modules for **1.20.1, 1.20.4, 1.21, 1.21.1, 1.21.4, and 1.21.8**. The new project is independent of the old workspace contents.

## Build

Requirements: Java 21 and Gradle 8.12. Fabric Loom 1.10.1 is used because the requested 1.21.8 Fabric API requires Loom 1.10.1 and Gradle 8.12; Loom 1.7 / Gradle 8.8 cannot build that dependency combination.

```sh
./gradlew buildAllClients
```

Individual modules can be built with `./gradlew :v1_20_1:build`, `:v1_20_4:build`, `:v1_21:build`, `:v1_21_1:build`, `:v1_21_4:build`, or `:v1_21_8:build`. Jars are named `maazxclient-<minecraft-version>-1.0.0.jar`.

## Launcher

The lightweight .NET launcher provides Play, Mods, Settings, and About tabs. It detects supported installed game versions and existing Fabric profiles, copies only the selected client jar into a dedicated subfolder of the Minecraft mods directory, writes the selected client settings, selects a named profile in `launcher_profiles.json`, and opens the official launcher. Fabric Loader must already be installed for the selected game version. Set `MC_HOME` to override the detected game directory.

Build on Windows with `dotnet publish launcher/MaazXLauncher.csproj -c Release -r win-x64 --self-contained false -o build/Release`. A .NET 10 runtime is required. The Inno Setup script packages the launcher and version jars.

## Included behavior

- Version-independent Java utilities for config persistence, frame-rate-based quality recommendations, entity/frustum distance tests, greedy 2D rectangle merging, bounded light/biome caches, async worker pools, particle limits, state interning, and UI command batching.
- Version-specific Fabric client initialization and a compact FPS/render-distance/entity-distance HUD.
- The launcher manages installed Fabric profiles and third-party jar enable/disable state.
- CI builds and inspects each version jar on Linux and Windows, and packages a Windows installer for tagged releases.

## Performance scope

The adaptive policy steps down requested render distance, entity distance, particles, and shadow quality recommendations when sampled FPS falls below its target. It restores settings gradually when headroom is stable. It does **not** override Minecraft options automatically, promise a minimum FPS, change GPU shader/texture compression, or replace Minecraft's chunk renderer. A stable 60 FPS depends on hardware, drivers, world complexity, server behavior, and other installed mods; no client can guarantee it on arbitrary systems.

This repository is not a port of Sodium, Lithium, Starlight, FerriteCore, Noisium, More Culling, EntityCulling, Cull Leaves, LazyDFU, Clumps, or BadOptimizations/FastFurnace. Their names in `common/.../optimization` identify the optimization area only; those small utilities are original code and are not their full logic or mixins. No third-party mod jars are included. See [LICENSES/README.md](LICENSES/README.md) for upstream project credits and license sources before making any upstream-derived additions.

## License

Original MaazXClient project code is MIT-licensed. See [LICENSE](LICENSE) and [LICENSES/README.md](LICENSES/README.md).