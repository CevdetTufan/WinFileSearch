; WinFileSearch Installer Script
; Inno Setup 6.x

#define MyAppName "WinFileSearch"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Cevdet Tufan | wordivio.com"
#define MyAppURL "https://github.com/CevdetTufan/WinFileSearch"
#define MyAppExeName "WinFileSearch.UI.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
AppId={{8A7B9D4E-5F6C-4A3B-9E2D-1C8F7A6B5E4D}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}/issues
AppUpdatesURL={#MyAppURL}/releases
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
; Output settings
OutputDir=installer
OutputBaseFilename=WinFileSearch-Setup-{#MyAppVersion}
; Compression
Compression=lzma2/ultra64
SolidCompression=yes
; Installer appearance
SetupIconFile=src\WinFileSearch.UI\Resources\app.ico
WizardStyle=modern
WizardImageFile=compiler:WizModernImage.bmp
WizardSmallImageFile=compiler:WizModernSmallImage.bmp
; Privileges
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
; Uninstaller
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}
; Windows version
MinVersion=10.0

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode
Name: "startupicon"; Description: "Start with Windows"; GroupDescription: "Other tasks:"; Flags: unchecked

[Files]
; Main application files (from publish output)
Source: "publish\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userstartup}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: startupicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Registry]
; Register application path
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\App Paths\{#MyAppExeName}"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName}"; Flags: uninsdeletekey

[Code]
// Check if .NET 8 runtime is installed (for framework-dependent builds)
function IsDotNet8Installed(): Boolean;
var
  ResultCode: Integer;
begin
  // For self-contained builds, always return True
  Result := True;
end;

// Custom initialization
function InitializeSetup(): Boolean;
begin
  Result := True;
end;

// Cleanup on uninstall
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  AppDataPath: String;
begin
  if CurUninstallStep = usPostUninstall then
  begin
    // Ask user if they want to remove app data
    if MsgBox('Do you want to remove WinFileSearch data (database, settings, logs)?', 
              mbConfirmation, MB_YESNO) = IDYES then
    begin
      AppDataPath := ExpandConstant('{localappdata}\WinFileSearch');
      if DirExists(AppDataPath) then
        DelTree(AppDataPath, True, True, True);
    end;
  end;
end;
