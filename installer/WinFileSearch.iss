; WinFileSearch Installer Script
#define MyAppName "WinFileSearch"
#define MyAppVersion "1.2.0"
#define MyAppPublisher "Cevdet Tufan | wordivio.com"
#define MyAppURL "https://github.com/CevdetTufan/WinFileSearch"
#define MyAppExeName "WinFileSearch.UI.exe"

[Setup]
AppId={{8A7B9D4E-5F6C-4A3B-9E2D-1C8F7A6B5E4D}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}/issues
AppUpdatesURL={#MyAppURL}/releases
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription=Fast file search application
VersionInfoCopyright=Copyright (C) 2026 {#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputDir=..
OutputBaseFilename=WinFileSearch-Setup-{#MyAppVersion}
SetupIconFile=app.ico
WizardStyle=modern
PrivilegesRequired=lowest
MinVersion=10.0

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"

[Tasks]
Name: "desktopicon"; Description: "Create desktop shortcut"
Name: "startupicon"; Description: "Start with Windows"; Flags: unchecked

[Files]
Source: "..\publish\win-x64\*"; DestDir: "{app}"; Flags: recursesubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userstartup}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: startupicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Flags: nowait postinstall skipifsilent