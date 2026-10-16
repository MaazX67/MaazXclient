using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CmlLib.Core.Auth;
using CmlLib.Core.Auth.Microsoft;
using XboxAuthNet.Game;

namespace MaazXLauncher;

public partial class MainWindow : Window
{
    private readonly LauncherState _state = LauncherState.Load();
    private readonly MinecraftService _minecraft = new();
    private readonly ObservableCollection<ModEntry> _mods = [];
    private readonly JELoginHandler _loginHandler = JELoginHandlerBuilder.BuildDefault();
    private MSession? _session;
    private string _selectedInstanceId = "";
    private bool _loading;

    public MainWindow()
    {
        InitializeComponent();
        _loading = true;
        VersionPicker.ItemsSource = MinecraftService.SupportedVersions;
        VersionList.ItemsSource = MinecraftService.SupportedVersions;
        ModsList.ItemsSource = _mods;
        MinecraftDirectoryBox.Text = _state.MinecraftDirectory;
        MemorySlider.Value = _state.MemoryGb;
        EntityDistanceSlider.Value = _state.EntityDistance;
        AdaptiveTuningCheckBox.IsChecked = _state.AdaptiveTuning;
        _loading = false;

        _selectedInstanceId = _state.Instances[0].Id;
        RefreshInstances();
        RefreshActiveInstance();
        JavaPathText.Text = "Java runtime: " + MinecraftService.DetectJava();
        Navigate("Play");
    }

    private GameInstance? ActiveInstance => _state.Instances.FirstOrDefault(item => item.Id == _selectedInstanceId);

    private void Navigate_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string page }) Navigate(page);
    }

    private void Navigate(string page)
    {
        PlayView.Visibility = page == "Play" ? Visibility.Visible : Visibility.Collapsed;
        InstancesView.Visibility = page == "Instances" ? Visibility.Visible : Visibility.Collapsed;
        ModsView.Visibility = page == "Mods" ? Visibility.Visible : Visibility.Collapsed;
        DownloadsView.Visibility = page == "Downloads" ? Visibility.Visible : Visibility.Collapsed;
        SettingsView.Visibility = page == "Settings" ? Visibility.Visible : Visibility.Collapsed;
        PageTitle.Text = page;
        PageSubtitle.Text = page switch
        {
            "Play" => "Choose an instance and get into your world.",
            "Instances" => "Separate profiles, worlds, and mods.",
            "Mods" => "Control what loads in the selected instance.",
            "Downloads" => "Install Minecraft and Fabric into an instance.",
            _ => "Set your paths and performance preferences."
        };
        foreach (var button in new[] { PlayNav, InstancesNav, ModsNav, DownloadsNav, SettingsNav })
        {
            bool active = Equals(button.Tag, page);
            button.Background = active ? new SolidColorBrush(Color.FromRgb(48, 35, 64)) : Brushes.Transparent;
            button.Foreground = active ? (Brush)FindResource("Text") : (Brush)FindResource("Muted");
        }
    }

    private void RefreshInstances()
    {
        InstancePicker.ItemsSource = null;
        InstancePicker.ItemsSource = _state.Instances.ToList();
        InstanceList.ItemsSource = null;
        InstanceList.ItemsSource = _state.Instances.ToList();
        InstanceList.DisplayMemberPath = nameof(GameInstance.Name);
        var active = ActiveInstance ?? _state.Instances.FirstOrDefault();
        if (active is null) return;
        _selectedInstanceId = active.Id;
        InstancePicker.SelectedItem = _state.Instances.FirstOrDefault(item => item.Id == active.Id);
        InstanceList.SelectedItem = _state.Instances.FirstOrDefault(item => item.Id == active.Id);
        SidebarInstanceText.Text = $"{_state.Instances.Count} instance{(_state.Instances.Count == 1 ? "" : "s")}";
    }

    private void RefreshActiveInstance()
    {
        var instance = ActiveInstance;
        if (instance is null) return;
        _loading = true;
        VersionPicker.SelectedItem = instance.Version;
        _loading = false;
        HeroInstanceName.Text = instance.Name;
        HeroInstanceInfo.Text = $"Minecraft {instance.Version}  ·  Fabric";
        GamePathSummary.Text = _state.MinecraftDirectory;
        InstancePathSummary.Text = instance.GameDirectory;
        LoadMods();
    }

    private void LoadMods()
    {
        _mods.Clear();
        _mods.Add(new ModEntry
        {
            Name = "MaazXClient",
            Detail = "Built-in client · version-specific Fabric module",
            IsBuiltIn = true,
            IsEnabled = true
        });
        var instance = ActiveInstance;
        if (instance is null) return;
        string folder = Path.Combine(instance.GameDirectory, "mods");
        if (!Directory.Exists(folder)) return;
        foreach (string file in Directory.GetFiles(folder)
                     .Where(path => path.EndsWith(".jar", StringComparison.OrdinalIgnoreCase)
                         || path.EndsWith(".jar.disabled", StringComparison.OrdinalIgnoreCase))
                     .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase))
        {
            if (Path.GetFileName(file).StartsWith("maazxclient-", StringComparison.OrdinalIgnoreCase)) continue;
            bool disabled = file.EndsWith(".disabled", StringComparison.OrdinalIgnoreCase);
            _mods.Add(new ModEntry
            {
                Name = Path.GetFileName(file),
                Detail = disabled ? "Disabled in this instance" : "Enabled in this instance",
                FilePath = file,
                IsEnabled = !disabled
            });
        }
    }

    private void InstancePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loading || InstancePicker.SelectedItem is not GameInstance instance) return;
        _selectedInstanceId = instance.Id;
        RefreshActiveInstance();
    }

    private void InstanceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loading || InstanceList.SelectedItem is not GameInstance instance) return;
        _selectedInstanceId = instance.Id;
        _loading = true;
        InstancePicker.SelectedItem = _state.Instances.FirstOrDefault(item => item.Id == instance.Id);
        _loading = false;
        RefreshActiveInstance();
    }

    private void VersionPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loading || VersionPicker.SelectedItem is not string version) return;
        var instance = ActiveInstance;
        if (instance is null) return;
        instance.Version = version;
        _state.Save();
        HeroInstanceInfo.Text = $"Minecraft {version}  ·  Fabric";
    }

    private void CreateInstance_Click(object sender, RoutedEventArgs e)
    {
        string name = string.IsNullOrWhiteSpace(NewInstanceName.Text)
            ? $"Instance {_state.Instances.Count + 1:00}"
            : NewInstanceName.Text.Trim();
        string folderName = string.Concat(name.Select(character => Path.GetInvalidFileNameChars().Contains(character) ? '-' : character));
        var instance = new GameInstance
        {
            Name = name,
            Version = (string?)VersionPicker.SelectedItem ?? "1.21.8",
            GameDirectory = Path.Combine(_state.MinecraftDirectory, "MaazXInstances", $"{folderName}-{Guid.NewGuid():N}")
        };
        _state.Instances.Add(instance);
        _selectedInstanceId = instance.Id;
        _state.Save();
        NewInstanceName.Clear();
        RefreshInstances();
        RefreshActiveInstance();
        SetStatus($"Created {name}.", 0);
    }

    private void RemoveInstance_Click(object sender, RoutedEventArgs e)
    {
        var instance = ActiveInstance;
        if (instance is null || _state.Instances.Count <= 1) return;
        var answer = MessageBox.Show(
            $"Remove '{instance.Name}' from the launcher? Its game files, worlds, and saves will be kept on disk.",
            "Remove instance", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (answer != MessageBoxResult.Yes) return;
        _state.Instances.Remove(instance);
        _selectedInstanceId = _state.Instances[0].Id;
        _state.Save();
        RefreshInstances();
        RefreshActiveInstance();
        SetStatus($"Removed {instance.Name} from the instance list. Files were kept.", 0);
    }

    private async void InstallVersion_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string version }) await InstallForActiveInstance(version, launch: false);
    }

    private async void Launch_Click(object sender, RoutedEventArgs e)
    {
        var instance = ActiveInstance;
        if (instance is null) return;
        string version = (string?)VersionPicker.SelectedItem ?? instance.Version;
        string fabricProfile = await InstallForActiveInstance(version, launch: false);
        if (string.IsNullOrEmpty(fabricProfile)) return;

        try
        {
            SetBusy(true);
            StatusText.Text = "Waiting for Microsoft sign-in...";
            _session ??= await _loginHandler.AuthenticateInteractively();
            StatusText.Text = "Starting Minecraft...";
            var process = await _minecraft.LaunchAsync(instance, _state.MemoryGb, _session, fabricProfile);
            SetStatus($"Minecraft launched · {instance.Name}", 100);
            process.EnableRaisingEvents = true;
            process.Exited += (_, _) => Dispatcher.Invoke(() => SetStatus("Minecraft closed.", 0));
        }
        catch (Exception exception)
        {
            SetStatus("Launch failed: " + exception.Message, 0);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task<string> InstallForActiveInstance(string version, bool launch)
    {
        var instance = ActiveInstance;
        if (instance is null) return "";
        instance.Version = version;
        _state.Save();
        RefreshActiveInstance();
        try
        {
            SetBusy(true);
            StatusText.Text = $"Preparing Minecraft {version}...";
            var progress = new Progress<InstallProgress>(item =>
            {
                TaskProgress.Value = item.Percent;
                StatusText.Text = item.Message;
                FooterStatus.Text = item.Message.ToUpperInvariant();
            });
            string profile = await _minecraft.InstallAsync(instance, _state, progress);
            LoadMods();
            SetStatus($"Installed Minecraft {version} with Fabric.", 100);
            return profile;
        }
        catch (Exception exception)
        {
            SetStatus("Install failed: " + exception.Message, 0);
            return "";
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void SignIn_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            AccountButton.IsEnabled = false;
            AccountButton.Content = "Signing in...";
            _session = await _loginHandler.AuthenticateInteractively();
            AccountButton.Content = "Microsoft account connected";
            SetStatus("Microsoft account connected.", 0);
        }
        catch (Exception exception)
        {
            AccountButton.Content = "Sign in with Microsoft";
            SetStatus("Sign-in failed: " + exception.Message, 0);
        }
        finally
        {
            AccountButton.IsEnabled = true;
        }
    }

    private void ToggleMod_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: ModEntry { FilePath: not null } mod }) return;
        string destination = mod.IsEnabled ? mod.FilePath + ".disabled" : mod.FilePath[..^".disabled".Length];
        try
        {
            File.Move(mod.FilePath, destination);
            LoadMods();
            SetStatus($"Updated {mod.Name}.", 0);
        }
        catch (Exception exception)
        {
            SetStatus("Could not toggle mod: " + exception.Message, 0);
        }
    }

    private void AddMod_Click(object sender, RoutedEventArgs e)
    {
        var instance = ActiveInstance;
        if (instance is null) return;
        var picker = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Add mod to this instance",
            Filter = "Fabric mod jars (*.jar)|*.jar",
            Multiselect = true
        };
        if (picker.ShowDialog(this) != true) return;
        string modsDirectory = Path.Combine(instance.GameDirectory, "mods");
        Directory.CreateDirectory(modsDirectory);
        try
        {
            foreach (string source in picker.FileNames)
                File.Copy(source, Path.Combine(modsDirectory, Path.GetFileName(source)), overwrite: false);
            LoadMods();
            SetStatus($"Added {picker.FileNames.Length} mod file(s).", 0);
        }
        catch (IOException exception)
        {
            SetStatus("A mod with that filename already exists: " + exception.Message, 0);
        }
    }

    private void BrowseDirectory_Click(object sender, RoutedEventArgs e)
    {
        var picker = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "Choose Minecraft directory",
            InitialDirectory = Directory.Exists(_state.MinecraftDirectory) ? _state.MinecraftDirectory : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        };
        if (picker.ShowDialog(this) != true) return;
        MinecraftDirectoryBox.Text = picker.FolderName;
        GamePathSummary.Text = picker.FolderName;
    }

    private void MemorySlider_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (MemoryValue is not null) MemoryValue.Text = $"{(int)e.NewValue} GB";
    }

    private void EntityDistanceSlider_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (EntityDistanceValue is not null) EntityDistanceValue.Text = $"{(int)e.NewValue} blocks";
    }

    private void SaveSettings_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(MinecraftDirectoryBox.Text))
        {
            SetStatus("Choose a valid Minecraft directory.", 0);
            return;
        }
        string previousRoot = Path.GetFullPath(_state.MinecraftDirectory);
        string newRoot = Path.GetFullPath(MinecraftDirectoryBox.Text.Trim());
        _state.MinecraftDirectory = newRoot;
        _state.MemoryGb = (int)MemorySlider.Value;
        _state.EntityDistance = (int)EntityDistanceSlider.Value;
        _state.AdaptiveTuning = AdaptiveTuningCheckBox.IsChecked == true;
        foreach (var instance in _state.Instances)
        {
            string instancesRoot = Path.Combine(previousRoot, "MaazXInstances");
            string relativePath = Path.GetRelativePath(instancesRoot, instance.GameDirectory);
            if (relativePath != ".." && !relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal))
                instance.GameDirectory = Path.Combine(newRoot, "MaazXInstances", relativePath);
        }
        _state.Save();
        RefreshActiveInstance();
        SetStatus("Preferences saved.", 0);
    }

    private void SetBusy(bool busy)
    {
        LaunchButton.IsEnabled = !busy;
        VersionPicker.IsEnabled = !busy;
        DownloadsNav.IsEnabled = !busy;
        TaskProgress.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
        if (!busy && TaskProgress.Value >= 100) TaskProgress.Value = 0;
    }

    private void SetStatus(string message, double percent)
    {
        StatusText.Text = message;
        FooterStatus.Text = message.ToUpperInvariant();
        TaskProgress.Value = percent;
    }

    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void Maximize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}