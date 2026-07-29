#define PublishDir "U:\Users\mark\Documents\Visual Studio\publish\MiniLaunch"
#define AppExe "MiniLaunch.exe"
#define MyVersion "1.0.0"

[Setup]
AppId=MiniLaunch
AppName=MiniLaunch
AppVersion={#MyVersion}
AppPublisher=MiniSuite
DefaultDirName={localappdata}\MiniLaunch
DefaultGroupName=MiniLaunch
OutputDir={#PublishDir}
OutputBaseFilename=MiniLaunchSetup_v{#MyVersion}
SetupIconFile=Resources\MiniLaunch.ico
Compression=lzma
SolidCompression=yes
PrivilegesRequired=lowest
CloseApplications=yes

DisableProgramGroupPage=no
UninstallDisplayIcon={app}\{#AppExe}

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; Flags: unchecked
Name: "startmenuicon"; Description: "Create a Start Menu shortcut"; Flags: unchecked

[Files]
Source: "{#PublishDir}\{#AppExe}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PublishDir}\Readme.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PublishDir}\License.txt"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
; Main app (optional)
Name: "{group}\MiniLaunch"; Filename: "{app}\{#AppExe}"; Tasks: startmenuicon

; ALWAYS CREATE THIS (no Tasks)
Name: "{group}\Uninstall MiniLaunch"; Filename: "{uninstallexe}"

; Optional extras
Name: "{group}\MiniLaunch - README"; Filename: "notepad.exe"; Parameters: """{app}\Readme.txt"""; Tasks: startmenuicon
Name: "{group}\MiniLaunch - LICENSE"; Filename: "notepad.exe"; Parameters: """{app}\License.txt"""; Tasks: startmenuicon

; Desktop
Name: "{autodesktop}\MiniLaunch"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExe}"; Description: "Launch MiniLaunch"; WorkingDir: "{app}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{localappdata}\MiniLaunch"