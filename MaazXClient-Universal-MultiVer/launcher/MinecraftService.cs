using System.Diagnostics;
using System.Net.Http;
using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.ModLoaders.FabricMC;
using CmlLib.Core.ProcessBuilder;
using XboxAuthNet.Game;

namespace MaazXLauncher;

internal sealed class MinecraftService
{
    private const string ClientRelease = "https://github.com/MaazX67/MaazXclient/releases/download/v1.0.0-universal-multi";
    private static readonly HttpClient Http = new();

    public static readonly string[] SupportedVersions = ["1.20.1", "1.20.4", "1.21", "1.21.1", "1.21.4", "1.21.8"];

    public async Task<string> InstallAsync(GameInstance instance, LauncherState state, IProgress<InstallProgress> progress)
    {
        Directory.CreateDirectory(instance.GameDirectory);
        var path = new MinecraftPath(instance.GameDirectory);
        var launcher = new MinecraftLauncher(path);
        launcher.FileProgressChanged += (_, args) =>
        {
            double fraction = args.TotalTasks <= 0 ? 0 : (double)args.ProgressedTasks / args.TotalTasks;
            progress.Report(new InstallProgress($"{args.Name}  ·  {args.ProgressedTasks}/{args.TotalTasks}", fraction * 75));
        };
        await launcher.InstallAsync(instance.Version);

        progress.Report(new InstallProgress("Installing the matching Fabric loader", 78));
        var fabric = new FabricInstaller(Http);
        var loader = (await fabric.GetLoaders(instance.Version))
            .FirstOrDefault(item => item.Stable && !string.IsNullOrWhiteSpace(item.Version))
            ?? throw new InvalidOperationException($"Fabric does not list a stable loader for Minecraft {instance.Version}.");
        string loaderVersion = loader.Version!;
        string profile = await fabric.Install(instance.Version, loaderVersion, path);

        progress.Report(new InstallProgress("Installing MaazXClient into this instance", 90));
        string modsDirectory = Path.Combine(instance.GameDirectory, "mods");
        Directory.CreateDirectory(modsDirectory);
        string jarName = $"maazxclient-{instance.Version}-1.0.0.jar";
        string clientJar = Path.Combine(modsDirectory, jarName);
        if (!File.Exists(clientJar))
        {
            string bundledJar = Path.Combine(AppContext.BaseDirectory, "mods", jarName);
            if (File.Exists(bundledJar))
            {
                File.Copy(bundledJar, clientJar);
            }
            else
            {
                Uri source = new($"{ClientRelease}/{jarName}");
                byte[] bytes = await Http.GetByteArrayAsync(source);
                await File.WriteAllBytesAsync(clientJar, bytes);
            }
        }
        WriteClientConfig(instance.GameDirectory, state);
        progress.Report(new InstallProgress($"Ready · Fabric {loader.Version}", 100));
        return profile;
    }

    public async Task<Process> LaunchAsync(GameInstance instance, int memoryGb, MSession session, string fabricProfile)
    {
        var launcher = new MinecraftLauncher(new MinecraftPath(instance.GameDirectory));
        var options = new MLaunchOption
        {
            Session = session,
            MaximumRamMb = Math.Clamp(memoryGb, 2, 16) * 1024
        };
        Process process = await launcher.BuildProcessAsync(fabricProfile, options);
        process.Start();
        return process;
    }

    public static string DetectJava()
    {
        string? home = Environment.GetEnvironmentVariable("JAVA_HOME");
        string executable = OperatingSystem.IsWindows() ? "java.exe" : "java";
        if (!string.IsNullOrWhiteSpace(home))
        {
            string candidate = Path.Combine(home, "bin", executable);
            if (File.Exists(candidate)) return candidate;
        }
        string? path = Environment.GetEnvironmentVariable("PATH");
        return path?.Split(Path.PathSeparator)
            .Select(directory => Path.Combine(directory, executable))
            .FirstOrDefault(File.Exists) ?? "Java is managed by the launcher";
    }

    private static void WriteClientConfig(string gameDirectory, LauncherState state)
    {
        string configDirectory = Path.Combine(gameDirectory, "config");
        Directory.CreateDirectory(configDirectory);
        File.WriteAllText(Path.Combine(configDirectory, "maazxclient.properties"),
            $"entity_distance={Math.Clamp(state.EntityDistance, 8, 128)}\nrender_distance=12\nparticle_limit=50\nlod_distance=4\nasync_disk=true\nguarantee_60fps={state.AdaptiveTuning.ToString().ToLowerInvariant()}\nhud_enabled=true\n");
    }
}