using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MaazXLauncher;

internal static class Program
{
    private static readonly string[] SupportedVersions = ["1.20.1", "1.20.4", "1.21", "1.21.1", "1.21.4", "1.21.8"];
    private static readonly string MinecraftDirectory = FindMinecraftDirectory();
    private static LauncherSettings settings = LauncherSettings.Load();

    private static int Main()
    {
        Console.Title = "MaazXLauncher | Universal Fabric Client";
        while (true)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  MX  MAAZXCLIENT LAUNCHER");
            Console.ResetColor();
            Console.WriteLine($"  Game directory: {MinecraftDirectory}");
            Console.WriteLine($"  Version: {settings.Version}   RAM: {settings.RamGb} GB   60 FPS tuning: {(settings.Guarantee60Fps ? "On" : "Off")}");
            Console.WriteLine("\n  [1] Play   [2] Mods   [3] Settings   [4] About   [Q] Quit");
            Console.Write("\n  Select tab: ");
            switch (Console.ReadLine()?.Trim().ToLowerInvariant())
            {
                case "1": PlayMenu(); break;
                case "2": ModsMenu(); break;
                case "3": SettingsMenu(); break;
                case "4": AboutMenu(); break;
                case "q": return 0;
            }
        }
    }

    private static void PlayMenu()
    {
        Console.Clear();
        var installed = DetectInstalledVersions();
        Console.WriteLine("PLAY | Installed Minecraft versions");
        if (installed.Count == 0)
        {
            Console.WriteLine("No supported Minecraft versions are installed.");
            Pause();
            return;
        }

        for (int index = 0; index < installed.Count; index++)
            Console.WriteLine($"  [{index + 1}] {installed[index]}{(installed[index] == settings.Version ? "  (selected)" : "")}");
        Console.Write("Select version, or Enter to keep current: ");
        string? selection = Console.ReadLine();
        if (int.TryParse(selection, out int selected) && selected > 0 && selected <= installed.Count)
            settings.Version = installed[selected - 1];

        string? profile = FindFabricProfile(settings.Version);
        if (profile is null)
        {
            Console.WriteLine($"No Fabric profile for Minecraft {settings.Version} was found. Install Fabric Loader for that version first.");
            Pause();
            return;
        }

        try
        {
            InstallClientJar(settings.Version);
            SelectLauncherProfile(settings.Version, profile);
            settings.Save();
            StartOfficialLauncher();
            Console.WriteLine($"Selected MaazXClient {settings.Version} ({profile}) in the official launcher.");
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Launch setup failed: {exception.Message}");
        }
        Pause();
    }

    private static void ModsMenu()
    {
        string modsDirectory = Path.Combine(MinecraftDirectory, "mods");
        Directory.CreateDirectory(modsDirectory);
        while (true)
        {
            Console.Clear();
            Console.WriteLine("MODS | MaazX performance routines are built into MaazXClient.");
            Console.WriteLine("MaazXClient version jars are managed by the Play tab and are not third-party mods.");
            string[] mods = Directory.GetFiles(modsDirectory)
                .Where(path => path.EndsWith(".jar", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".jar.disabled", StringComparison.OrdinalIgnoreCase))
                .Where(path => !Path.GetFileName(path).StartsWith("maazxclient-", StringComparison.OrdinalIgnoreCase))
                .Order(StringComparer.OrdinalIgnoreCase).ToArray();
            if (mods.Length == 0) Console.WriteLine("No external mod jars found.");
            for (int index = 0; index < mods.Length; index++)
                Console.WriteLine($"  [{index + 1}] {(mods[index].EndsWith(".disabled", StringComparison.OrdinalIgnoreCase) ? "Off " : "On  ")} {Path.GetFileName(mods[index])}");
            Console.WriteLine("\nEnter a number to toggle a jar, or Enter to return.");
            if (!int.TryParse(Console.ReadLine(), out int selected)) return;
            if (selected < 1 || selected > mods.Length) continue;
            string current = mods[selected - 1];
            string destination = current.EndsWith(".disabled", StringComparison.OrdinalIgnoreCase)
                ? current[..^".disabled".Length]
                : current + ".disabled";
            File.Move(current, destination);
        }
    }

    private static void SettingsMenu()
    {
        Console.Clear();
        Console.WriteLine("SETTINGS");
        Console.WriteLine($"  [1] RAM: {settings.RamGb} GB (1-4)");
        Console.WriteLine($"  [2] 60 FPS dynamic tuning: {(settings.Guarantee60Fps ? "On" : "Off")}");
        Console.WriteLine($"  [3] Entity distance: {settings.EntityDistance} blocks (8-128)");
        Console.WriteLine($"  [4] Java: {FindJava() ?? "not detected; launcher default"}");
        Console.WriteLine("  Atlas target: 4096  |  Renderer: Automatic");
        Console.Write("Choose a setting or Enter to return: ");
        switch (Console.ReadLine()?.Trim())
        {
            case "1":
                Console.Write("RAM in GB (1-4): ");
                if (int.TryParse(Console.ReadLine(), out int ram)) settings.RamGb = Math.Clamp(ram, 1, 4);
                break;
            case "2": settings.Guarantee60Fps = !settings.Guarantee60Fps; break;
            case "3":
                Console.Write("Entity distance in blocks (8-128): ");
                if (int.TryParse(Console.ReadLine(), out int distance)) settings.EntityDistance = Math.Clamp(distance, 8, 128);
                break;
        }
        settings.Save();
        Pause();
    }

    private static void AboutMenu()
    {
        Console.Clear();
        Console.WriteLine("ABOUT | MaazXClient Universal");
        Console.WriteLine("Supported modules: " + string.Join(", ", SupportedVersions));
        Console.WriteLine("Open-source project; see README.md and LICENSES/ for implementation notes and upstream credits.");
        Console.WriteLine("Performance varies by hardware, drivers, world, and other mods; 60 FPS is a tuning target, not a guarantee.");
        Pause();
    }

    private static List<string> DetectInstalledVersions()
    {
        string versionsDirectory = Path.Combine(MinecraftDirectory, "versions");
        if (!Directory.Exists(versionsDirectory)) return [];
        var installed = new List<string>();
        foreach (string directory in Directory.GetDirectories(versionsDirectory))
        {
            string manifest = Path.Combine(directory, "version.json");
            if (!File.Exists(manifest)) continue;
            try
            {
                string? id = JsonNode.Parse(File.ReadAllText(manifest))?["id"]?.GetValue<string>();
                if (id is not null && SupportedVersions.Contains(id) && !installed.Contains(id)) installed.Add(id);
            }
            catch (JsonException) { }
        }
        return installed.OrderBy(version => Array.IndexOf(SupportedVersions, version)).ToList();
    }

    private static string? FindFabricProfile(string minecraftVersion)
    {
        string versionsDirectory = Path.Combine(MinecraftDirectory, "versions");
        if (!Directory.Exists(versionsDirectory)) return null;
        string suffix = "-" + minecraftVersion;
        return Directory.GetDirectories(versionsDirectory)
            .Select(Path.GetFileName)
            .Where(name => name is not null && name.StartsWith("fabric-loader-", StringComparison.OrdinalIgnoreCase) && name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            .Order(StringComparer.OrdinalIgnoreCase).FirstOrDefault();
    }

    private static void InstallClientJar(string minecraftVersion)
    {
        string fileName = $"maazxclient-{minecraftVersion}-1.0.0.jar";
        string installedJar = Path.Combine(AppContext.BaseDirectory, "mods", fileName);
        string? jar = File.Exists(installedJar) ? installedJar : null;
        if (jar is null)
        {
            string root = FindProjectRoot();
            string module = "v" + minecraftVersion.Replace('.', '_');
            string libs = Path.Combine(root, module, "build", "libs");
            jar = Directory.Exists(libs)
                ? Directory.GetFiles(libs, $"maazxclient-{minecraftVersion}-*.jar").FirstOrDefault()
                : null;
        }
        if (jar is null) throw new FileNotFoundException($"Build the {minecraftVersion} module first; no client jar was found.");
        string mods = Path.Combine(MinecraftDirectory, "mods");
        string clientMods = Path.Combine(mods, "maazxclient");
        Directory.CreateDirectory(clientMods);
        string destination = Path.Combine(clientMods, "maazxclient.jar");
        File.Copy(jar, destination, overwrite: true);
        string config = Path.Combine(MinecraftDirectory, "config", "maazxclient.properties");
        Directory.CreateDirectory(Path.GetDirectoryName(config)!);
        File.WriteAllText(config,
            $"entity_distance={settings.EntityDistance}\nrender_distance=12\nparticle_limit=50\nlod_distance=4\nasync_disk=true\nguarantee_60fps={settings.Guarantee60Fps.ToString().ToLowerInvariant()}\nhud_enabled=true\n");
    }

    private static void SelectLauncherProfile(string minecraftVersion, string fabricProfile)
    {
        string path = Path.Combine(MinecraftDirectory, "launcher_profiles.json");
        if (!File.Exists(path)) throw new FileNotFoundException("Official launcher profile file was not found.", path);
        JsonObject root = JsonNode.Parse(File.ReadAllText(path)) as JsonObject ?? throw new InvalidDataException("Launcher profile JSON is invalid.");
        JsonObject profiles = root["profiles"] as JsonObject ?? new JsonObject();
        root["profiles"] = profiles;
        string profileId = "maazxclient-" + minecraftVersion.Replace('.', '-');
        profiles[profileId] = new JsonObject
        {
            ["name"] = "MaazXClient " + minecraftVersion,
            ["type"] = "custom",
            ["created"] = DateTimeOffset.UtcNow.ToString("O"),
            ["lastUsed"] = DateTimeOffset.UtcNow.ToString("O"),
            ["lastVersionId"] = fabricProfile,
            ["gameDir"] = MinecraftDirectory,
            ["javaArgs"] = $"-XX:+UseZGC -Xmx{settings.RamGb}G -Dmaazx.guarantee60={settings.Guarantee60Fps.ToString().ToLowerInvariant()}"
        };
        root["selectedProfile"] = profileId;
        File.WriteAllText(path, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    private static void StartOfficialLauncher()
    {
        string? launcher = OperatingSystem.IsWindows()
            ? new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Minecraft Launcher", "MinecraftLauncher.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Minecraft Launcher", "MinecraftLauncher.exe")
            }.FirstOrDefault(File.Exists)
            : FindCommand("minecraft-launcher");
        if (launcher is null) throw new FileNotFoundException("Official Minecraft Launcher was not found.");
        Process.Start(new ProcessStartInfo(launcher) { UseShellExecute = true });
    }

    private static string? FindJava()
    {
        string? javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
        if (javaHome is not null)
        {
            string candidate = Path.Combine(javaHome, "bin", OperatingSystem.IsWindows() ? "java.exe" : "java");
            if (File.Exists(candidate)) return candidate;
        }
        string[] candidates = OperatingSystem.IsWindows()
            ? [@"D:\Program Files\graalvm-jdk-21.0.12+7.1\bin\java.exe", @"C:\Program Files\Eclipse Adoptium\jdk-21\bin\java.exe"]
            : ["/usr/bin/java", "/usr/local/bin/java"];
        return candidates.FirstOrDefault(File.Exists) ?? FindCommand("java");
    }

    private static string FindMinecraftDirectory()
    {
        string? configured = Environment.GetEnvironmentVariable("MC_HOME");
        if (!string.IsNullOrWhiteSpace(configured)) return Path.GetFullPath(configured);
        if (OperatingSystem.IsWindows()) return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft");
        if (OperatingSystem.IsMacOS()) return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Application Support", "minecraft");
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".minecraft");
    }

    private static string FindProjectRoot()
    {
        for (DirectoryInfo? current = new(AppContext.BaseDirectory); current is not null; current = current.Parent)
            if (Directory.Exists(Path.Combine(current.FullName, "v1_20_1"))) return current.FullName;
        for (DirectoryInfo? current = new(Directory.GetCurrentDirectory()); current is not null; current = current.Parent)
            if (Directory.Exists(Path.Combine(current.FullName, "v1_20_1"))) return current.FullName;
        throw new DirectoryNotFoundException("Could not locate the MaazXClient project root.");
    }

    private static string? FindCommand(string command)
    {
        string? path = Environment.GetEnvironmentVariable("PATH");
        if (path is null) return null;
        return path.Split(Path.PathSeparator)
            .Select(directory => Path.Combine(directory, command))
            .FirstOrDefault(File.Exists);
    }

    private static void Pause()
    {
        Console.WriteLine("\nPress Enter to continue.");
        Console.ReadLine();
    }
}

internal sealed class LauncherSettings
{
    public string Version { get; set; } = "1.21.8";
    public int RamGb { get; set; } = 2;
    public int EntityDistance { get; set; } = 32;
    public bool Guarantee60Fps { get; set; } = true;

    private static string FilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MaazX", "launcher.json");

    public static LauncherSettings Load()
    {
        try
        {
            if (File.Exists(FilePath)) return JsonSerializer.Deserialize<LauncherSettings>(File.ReadAllText(FilePath)) ?? new();
        }
        catch (JsonException) { }
        return new LauncherSettings();
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }
}