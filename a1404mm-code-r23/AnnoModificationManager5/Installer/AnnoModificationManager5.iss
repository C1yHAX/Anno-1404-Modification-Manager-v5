; Inno Setup script for the Anno 1404 Modification Manager 5
; -----------------------------------------------------------
; Build the solution first (Debug or Release). All projects output into
;   ..\AnnoModificationManager5\bin\Debug
; which is what SourceDir points to below. Then compile this script with
; Inno Setup 6 (https://jrsoftware.org/isdl.php) -> produces Output\AMM5_Setup.exe
;
; To build from the command line:
;   "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" AnnoModificationManager5.iss

#define AppName "Anno 1404 Modification Manager 5"
#define Publisher "Anno 1404 Modding Community"
#define ExeName "AnnoModificationManager5.exe"
; Folder that contains the built files (relative to this .iss):
#define SourceDir "..\AnnoModificationManager5\bin\Release"
; Read the product version straight from the built exe:
#define AppVersion GetFileVersion(SourceDir + "\" + ExeName)

[Setup]
; A stable AppId so updates install over the previous version and uninstall works.
AppId={{B7E4C2A0-9F31-4D58-A6E1-3C5D8F2A4B60}}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#Publisher}
DefaultDirName={autopf}\Anno 1404 Modification Manager 5
DefaultGroupName=Anno 1404 Modification Manager 5
DisableProgramGroupPage=yes
OutputDir=Output
OutputBaseFilename=AMM5_Setup_{#AppVersion}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
; 32-bit app -> install into Program Files (x86) on 64-bit Windows.
PrivilegesRequired=admin
UninstallDisplayIcon={app}\{#ExeName}

[Languages]
Name: "german"; MessagesFile: "compiler:Languages\German.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Bundle the whole build output, excluding debug/VS-only artifacts.
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs ignoreversion; \
    Excludes: "*.pdb,*.vshost.*,*.rar,*.lref,*.cache"

[Icons]
Name: "{group}\Anno 1404 Modification Manager 5"; Filename: "{app}\{#ExeName}"
Name: "{group}\Development Tools"; Filename: "{app}\DevelopmentTools.exe"
Name: "{group}\RDA Explorer"; Filename: "{app}\RDAExplorerGUI.exe"
Name: "{group}\{cm:UninstallProgram,{#AppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\Anno 1404 Modification Manager 5"; Filename: "{app}\{#ExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#ExeName}"; Description: "{cm:LaunchProgram,{#AppName}}"; Flags: nowait postinstall skipifsilent

[Code]
// Require .NET Framework 4.7.2 (Release >= 461808) and warn if missing.
function IsDotNet472OrLater: Boolean;
var
  release: Cardinal;
begin
  Result := RegQueryDWordValue(HKLM, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', release)
            and (release >= 461808);
end;

function InitializeSetup: Boolean;
begin
  Result := True;
  if not IsDotNet472OrLater then
  begin
    if MsgBox('Microsoft .NET Framework 4.7.2 (oder neuer) wird benoetigt und wurde nicht gefunden.'
              + #13#10 + 'Bitte zuerst .NET Framework 4.7.2 installieren.'
              + #13#10#13#10 + 'Trotzdem mit der Installation fortfahren?',
              mbConfirmation, MB_YESNO) = IDNO then
      Result := False;
  end;
end;
