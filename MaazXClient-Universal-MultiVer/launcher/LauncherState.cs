using System.Text.Json;

namespace MaazXLauncher;

internal sealed class LauncherState
{
    public string MinecraftDirectory { get; set; } = LauncherState.DefaultMinecraftDirectory;
    public int MemoryGb { get; set; } = 4;
    public int EntityDistance { get; set; } = 32;
    public bool AdaptiveTuning { get; set; } = true;
    public List<GameInstance> Instances { get; set; } = [];

    public static string DefaultMinecraftDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft");

    private static string StatePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MaazX", "launcher.json");

    public static LauncherState Load()
    {
        LauncherState state;
        try
        {
            state = File.Exists(StatePath)
                ? JsonSerializer.Deserialize<LauncherState>(File.ReadAllText(StatePath)) ?? new LauncherState()
                : new LauncherState();
        }
        catch (JsonException)
        {
            state = new LauncherState();
        }

        state.MinecraftDirectory = string.IsNullOrWhiteSpace(state.MinecraftDirectory)
            ? DefaultMinecraftDirectory
            : Path.GetFullPath(state.MinecraftDirectory);
        state.Instances ??= [];
        if (state.Instances.Count == 0)
        {
            state.Instances.Add(new GameInstance
            {
                Name = "Main",
                Version = "1.21.8",
                GameDirectory = Path.Combine(state.MinecraftDirectory, "MaazXInstances", "Main")
            });
        }
        return state;
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(StatePath)!);
        File.WriteAllText(StatePath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }
}

internal sealed class GameInstance
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "New instance";
    public string Version { get; set; } = "1.21.8";
    public string GameDirectory { get; set; } = "";
}

internal sealed class ModEntry
{
    public string Name { get; init; } = "";
    public string Detail { get; init; } = "";
    public string? FilePath { get; init; }
    public bool IsEnabled { get; init; }
    public bool IsBuiltIn { get; init; }
}

internal sealed record InstallProgress(string Message, double Percent);