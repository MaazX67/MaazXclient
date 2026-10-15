#define ProductName "MaazXClient Universal"
#define ProductVersion "1.0.0"

[Setup]
AppId={{B06F4CF6-F6A2-46DA-B851-0FA3750CFF38}
AppName={#ProductName}
AppVersion={#ProductVersion}
DefaultDirName={autopf}\MaazX
DefaultGroupName=MaazXClient
OutputDir=..\build\installer
OutputBaseFilename=MaazXLauncher-Setup
ArchitecturesInstallIn64BitMode=x64
Compression=lzma2
SolidCompression=yes
WizardStyle=modern

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked

[Files]
Source: "..\build\Release\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\assets\icon.png"; DestDir: "{app}\assets"; Flags: ignoreversion
Source: "..\v1_20_1\build\libs\maazxclient-1.20.1-1.0.0.jar"; DestDir: "{app}\mods"; Flags: ignoreversion
Source: "..\v1_20_4\build\libs\maazxclient-1.20.4-1.0.0.jar"; DestDir: "{app}\mods"; Flags: ignoreversion
Source: "..\v1_21\build\libs\maazxclient-1.21-1.0.0.jar"; DestDir: "{app}\mods"; Flags: ignoreversion
Source: "..\v1_21_1\build\libs\maazxclient-1.21.1-1.0.0.jar"; DestDir: "{app}\mods"; Flags: ignoreversion
Source: "..\v1_21_4\build\libs\maazxclient-1.21.4-1.0.0.jar"; DestDir: "{app}\mods"; Flags: ignoreversion
Source: "..\v1_21_8\build\libs\maazxclient-1.21.8-1.0.0.jar"; DestDir: "{app}\mods"; Flags: ignoreversion

[Icons]
Name: "{group}\MaazXClient Launcher"; Filename: "{app}\MaazXLauncher.exe"
Name: "{autodesktop}\MaazXClient Launcher"; Filename: "{app}\MaazXLauncher.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\MaazXLauncher.exe"; Description: "Launch MaazXClient Launcher"; Flags: postinstall nowait skipifsilent