# MaazXClient Universal MultiVer

A Fabric client project with separate Minecraft modules for **1.20.1, 1.20.4, 1.21, 1.21.1, 1.21.4, and 1.21.8**. The new project is independent of the old workspace contents.

## Build

Requirements: Java 21 and Gradle 8.12. Fabric Loom 1.10.1 is used because the requested 1.21.8 Fabric API requires Loom 1.10.1 and Gradle 8.12; Loom 1.7 / Gradle 8.8 cannot build that dependency combination.

```sh
./gradlew buildAllClients
```

Individual modules can be built with `./gradlew :v1_20_1:build`, `:v1_20_4:build`, `:v1_21:build`, `:v1_21_1:build`, `:v1_21_4:build`, or `:v1_21_8:build`. Jars are named `maazxclient-<minecraft-version>-1.0.0.jar`.

## Launcher

MaazXLauncher is a Windows WPF desktop app with Play, Instances, Mods, Downloads, and Settings views. It creates isolated game directories, installs supported Minecraft versions and a stable Fabric loader in-app, downloads the matching MaazX jar, signs in through Microsoft's official account flow, and launches the selected profile directly. The mod view can import and enable/disable jar files per instance. Instance removal only unregisters the profile; it does not delete saves or game files.

Build on Windows with `dotnet publish launcher/MaazXLauncher.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o build/Release`. CI publishes the desktop launcher and packages all supported jars into `MaazXLauncher-Setup.exe` with Inno Setup. The launcher uses CmlLib.Core and CmlLib.Core.Auth.Microsoft (MIT) for Mojang/Fabric installation and Microsoft authentication. Set the Minecraft directory in Settings to control the parent directory for managed instances.

## Included behavior

- Version-independent Java utilities for config persistence, frame-rate-based quality recommendations, entity/frustum distance tests, greedy 2D rectangle merging, bounded light/biome caches, async worker pools, particle limits, state interning, and UI command batching.
- Version-specific Fabric client initialization and a compact FPS/render-distance/entity-distance HUD.
- The launcher manages version downloads, Fabric installation, Microsoft sign-in, instance directories, and third-party jar enable/disable state.
- CI builds all version jars sequentially, publishes Windows/Linux launcher artifacts, packages a Windows installer, and updates the universal GitHub Release.

## Performance scope

The adaptive policy steps down requested render distance, entity distance, particles, and shadow quality recommendations when sampled FPS falls below its target. It restores settings gradually when headroom is stable. It does **not** override Minecraft options automatically, promise a minimum FPS, change GPU shader/texture compression, or replace Minecraft's chunk renderer. A stable 60 FPS depends on hardware, drivers, world complexity, server behavior, and other installed mods; no client can guarantee it on arbitrary systems.

This repository is not a port of Sodium, Lithium, Starlight, FerriteCore, Noisium, More Culling, EntityCulling, Cull Leaves, LazyDFU, Clumps, or BadOptimizations/FastFurnace. Their names in `common/.../optimization` identify the optimization area only; those small utilities are original code and are not their full logic or mixins. No third-party mod jars are included. See [LICENSES/README.md](LICENSES/README.md) for upstream project credits and license sources before making any upstream-derived additions.

## License

Original MaazXClient project code is MIT-licensed. See [LICENSE](LICENSE) and [LICENSES/README.md](LICENSES/README.md).