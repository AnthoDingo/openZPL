; openZPL - script Inno Setup
;
; Avant de compiler ce script, publier l'application en mode autonome
; (self-contained) depuis la racine du depot :
;
;   dotnet publish src\openZPL.csproj -c Release -r win-x64 --self-contained true ^
;       -o src\bin\Release\net10.0-windows\win-x64\publish
;
; Puis compiler avec l'EDI Inno Setup ou en ligne de commande :
;
;   iscc Setup\openZPL.iss
;
; Le mode autonome evite d'exiger l'installation prealable du runtime .NET
; Desktop chez l'utilisateur final.
;
; Mise a jour entre versions (section [Code]) : l'installateur detecte une
; installation existante (meme AppId), refuse de revenir a une version plus
; ancienne, affiche un message de bienvenue adapte et repart d'un dossier
; d'installation propre pour ne pas laisser trainer d'anciens fichiers.
; Necessite Inno Setup 6.3 ou superieur (constante HKA utilisable en script).

#define MyAppName "openZPL"
; MyAppVersion peut etre injecte depuis la ligne de commande (/DMyAppVersion=X.Y.Z,
; utilise par le workflow GitHub Actions) ; a defaut, valeur par defaut pour compilation locale.
#ifndef MyAppVersion
#define MyAppVersion "1.0.0"
#endif
#define MyAppPublisher "AnthoDingo"
#define MyAppURL "https://github.com/AnthoDingo/openZPL"
#define MyAppExeName "openZPL.exe"
#define MyPublishDir "..\src\bin\Release\net10.0-windows\win-x64\publish"

[Setup]
; Identifiant unique de l'application : ne jamais changer entre deux versions,
; sinon les mises a jour ne se detecteront plus comme telles.
AppId={{B779D181-3D7F-41C7-8FCB-D3A6998F3CD5}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}/issues
AppUpdatesURL={#MyAppURL}/releases
VersionInfoVersion={#MyAppVersion}

; L'installateur ecrit sous HKA (HKLM ou HKCU selon le mode choisi) : aucune
; elevation n'est necessaire en installation utilisateur. Le dialogue laisse
; neanmoins le choix d'une installation machine si souhaite.
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog

DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes

LicenseFile=..\LICENSE
SetupIconFile=..\src\Resources\openZPL.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}

OutputDir=Output
OutputBaseFilename={#MyAppName}-Setup-{#MyAppVersion}

Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern

ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.17763

; Rafraichit l'icone et les caches de l'Explorateur apres modification des
; associations de fichiers (installation et desinstallation).
ChangesAssociations=yes

[Languages]
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "{#MyPublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

; Association de l'extension .ozpl avec l'application (ProgID "openZPL.Label").
; HKA pointe vers HKLM en installation machine ou HKCU en installation
; utilisateur.
[Registry]
Root: HKA; Subkey: "Software\Classes\.ozpl"; ValueType: string; ValueName: ""; ValueData: "openZPL.Label"; Flags: uninsdeletevalue
Root: HKA; Subkey: "Software\Classes\openZPL.Label"; ValueType: string; ValueName: ""; ValueData: "Etiquette openZPL"; Flags: uninsdeletekey
Root: HKA; Subkey: "Software\Classes\openZPL.Label\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"",0"
Root: HKA; Subkey: "Software\Classes\openZPL.Label\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#MyAppName}}"; Flags: nowait postinstall skipifsilent

[Code]
{ Compare deux numeros de version "X.Y.Z..." segment par segment et
  numeriquement (une comparaison textuelle jugerait "1.10.0" inferieur a
  "1.9.0"). Renvoie <0 si V1<V2, 0 si egales, >0 si V1>V2. }
function CompareVersion(V1, V2: String): Integer;
var
  P, N1, N2: Integer;
begin
  Result := 0;
  while (Result = 0) and ((V1 <> '') or (V2 <> '')) do
  begin
    P := Pos('.', V1);
    if P > 0 then
    begin
      N1 := StrToIntDef(Copy(V1, 1, P - 1), 0);
      Delete(V1, 1, P);
    end
    else
    begin
      N1 := StrToIntDef(V1, 0);
      V1 := '';
    end;

    P := Pos('.', V2);
    if P > 0 then
    begin
      N2 := StrToIntDef(Copy(V2, 1, P - 1), 0);
      Delete(V2, 1, P);
    end
    else
    begin
      N2 := StrToIntDef(V2, 0);
      V2 := '';
    end;

    if N1 < N2 then Result := -1
    else if N1 > N2 then Result := 1;
  end;
end;

{ Version de l'installation existante (cle ecrite automatiquement par Inno
  Setup a l'installation precedente), chaine vide si openZPL n'est pas deja
  installe. HKA vise HKLM ou HKCU selon le mode d'installation choisi
  precedemment, comme dans [Registry] ci-dessus. }
function GetInstalledVersion(): String;
begin
  if not RegQueryStringValue(HKA,
       'Software\Microsoft\Windows\CurrentVersion\Uninstall\{B779D181-3D7F-41C7-8FCB-D3A6998F3CD5}_is1',
       'DisplayVersion', Result) then
    Result := '';
end;

{ Refuse d'installer une version plus ancienne que celle deja presente, pour
  ne jamais ecraser une mise a jour par erreur avec un installateur perime. }
function InitializeSetup(): Boolean;
var
  InstalledVersion: String;
begin
  Result := True;
  InstalledVersion := GetInstalledVersion();
  if (InstalledVersion <> '') and (CompareVersion(InstalledVersion, '{#MyAppVersion}') > 0) then
  begin
    MsgBox('La version ' + InstalledVersion + ' de {#MyAppName} est deja installee sur ce poste.' + #13#10 +
           'Cet installateur ne contient que la version {#MyAppVersion} et ne peut pas revenir en arriere.',
           mbError, MB_OK);
    Result := False;
  end;
end;

{ Message d'accueil adapte lorsqu'une version differente est deja installee. }
procedure InitializeWizard();
var
  InstalledVersion: String;
begin
  InstalledVersion := GetInstalledVersion();
  if (InstalledVersion <> '') and (InstalledVersion <> '{#MyAppVersion}') then
    WizardForm.WelcomeLabel2.Caption :=
      'Cet assistant va mettre a jour {#MyAppName} de la version ' + InstalledVersion +
      ' vers la version {#MyAppVersion}.' + #13#10 + #13#10 +
      'Vos preferences et vos etiquettes ne sont pas affectees.';
end;

{ Juste avant la copie des fichiers : si une version differente est deja
  installee au meme emplacement, repart d'un dossier propre pour ne pas
  laisser trainer d'anciens fichiers que la nouvelle version ne contient
  plus (DLL renommee ou retiree entre deux versions, par exemple). Les
  preferences utilisateur sont hors du dossier d'installation (voir
  Services/AppSettings.cs, qui les ecrit dans %AppData%\openZPL), rien n'est
  perdu. Le controle sur
  la presence de l'executable evite de purger un dossier qui ne serait pas
  reellement une installation d'openZPL. }
procedure CurStepChanged(CurStep: TSetupStep);
var
  InstalledVersion: String;
begin
  if CurStep = ssInstall then
  begin
    InstalledVersion := GetInstalledVersion();
    if (InstalledVersion <> '') and (InstalledVersion <> '{#MyAppVersion}')
       and FileExists(ExpandConstant('{app}\{#MyAppExeName}')) then
      DelTree(ExpandConstant('{app}'), True, True, True);
  end;
end;
