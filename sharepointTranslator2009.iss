[ISSI]
#define ISSI_LanguagesShowAll

; ISSI Languages
#define ISSI_English
#define ISSI_French
#define ISSI_Spanish
#define ISSI_Korean
#define ISSI_Arabic
#define ISSI_Japanese
#define ISSI_ChineseTrad
#define ISSI_Portuguese
#define ISSI_German
#define ISSI_Italian
#define ISSI_Russian
#define ISSI_Dutch
#define ISSI_Ukrainian
#define ISSI_Polish

;; Activation of Serial Generation
;;#define ISSI_GenerateSerial
;;#define ISSI_GenerateSerial_AppName "Lassonde"

#define OCEANIK_VERSION "5.2.09.01"

[Setup]
OutputDir=.\Oceanik
VersionInfoVersion=5.2.09.01
VersionInfoCompany=ALPHAMOSAIK
VersionInfoDescription=Oceanik
VersionInfoTextVersion=5.2.09.01
VersionInfoCopyright=Alphamosaik 2010
SetupLogging=true
VersionInfoProductName=Oceanik
VersionInfoProductVersion=5.2
MinVersion=0,5.02.3790
OnlyBelowVersion=0,0
AppName=Oceanik
AppVerName=Oceanik 2010
DefaultDirName={pf}\Alphamosaik\Oceanik
DefaultGroupName=Oceanik
WizardImageFile=.\MM-install.bmp
WizardSmallImageFile=.\MM-install-MSI_mini.bmp
AppCopyright=WARNING: This computer program is protected by copyright law and international treaties. Unauthorized duplication or distribution of this program, or any portion of it, may result in severe civil or criminal penalties, and will be prosecuted to the maximum extent possible under the law.
InfoBeforeFile=.\SharepointTranslator2009\WarningBeforeInstallation.rtf
LicenseFile=.\SharepointTranslator2009\NDA.rtf
ArchitecturesAllowed=x86 x64 ia64
ArchitecturesInstallIn64BitMode=x64 ia64
PrivilegesRequired=admin
AppID=SharePoint Translator


; ISSI Iclude
#define ISSI_IncludePath "..\..\..\..\Common\Main\tools\Inno Setup 5\ISSI"
#include ISSI_IncludePath+"\_issi.isi"

#include "scripts\products.iss"
#include "scripts\products\winversion.iss"
#include "scripts\products\fileversion.iss"
#include "scripts\products\msi31.iss"
#include "scripts\products\dotnetfx35sp1.iss"

[Files]
Source: ..\..\..\..\Common\Main\tools\Inno Setup 5\ISSI\Include\isxdl\isxdl.dll; Flags: dontcopy
Source: .\SharepointTranslator2009\adminPagesToTranslate.txt; DestDir: {app}; Flags: overwritereadonly ignoreversion
Source: ..\Doc\Help Oceanik 2010.pdf; DestDir: {app}; Flags: overwritereadonly ignoreversion
Source: .\SharepointTranslator2009\Help_and_Support.ico; DestDir: {app}; Flags: overwritereadonly ignoreversion
Source: .\InstallHelper\bin\Release\InstallHelper.dll; DestDir: {app}; Flags: overwritereadonly ignoreversion
Source: .\SharepointTranslator2009\NDA.rtf; DestDir: {app}; Flags: overwritereadonly ignoreversion
Source: .\SharepointTranslator2009\pagesNotToTranslate.txt; DestDir: {app}; Flags: overwritereadonly ignoreversion
Source: .\SharepointTranslator2009\SDK - Sharepoint Translator.chm; DestDir: {app}; Flags: overwritereadonly ignoreversion
Source: .\SharepointTranslator2009\translations.txt; DestDir: {app}; Flags: overwritereadonly ignoreversion
Source: .\SharepointTranslator2009\WarningBeforeInstallation.rtf; DestDir: {app}; Flags: overwritereadonly ignoreversion
Source: ..\..\..\..\Common\Main\source\Alphamosaik.Common.Library\Alphamosaik.Common.Library\bin\Release\Alphamosaik.Common.Library.dll; DestDir: {app}; Flags: gacinstall overwritereadonly ignoreversion; StrongAssemblyName: Alphamosaik.Common.Library, Version=1.0.0.0, Culture=neutral, PublicKeyToken=eb9f205c2f6f15be, ProcessorArchitecture=MSIL
Source: ..\..\..\..\Common\Main\source\Alphamosaik.Common.SharePoint.Library\bin\Release\Alphamosaik.Common.SharePoint.Library.dll; DestDir: {app}; Flags: gacinstall overwritereadonly ignoreversion; StrongAssemblyName: Alphamosaik.Common.SharePoint.Library, Version=1.0.0.0, Culture=neutral, PublicKeyToken=eb9f205c2f6f15be, ProcessorArchitecture=MSIL
Source: .\Translator.Common.Library\bin\Release\Translator.Common.Library.dll; DestDir: {app}; Flags: overwritereadonly ignoreversion gacinstall; StrongAssemblyName: Translator.Common.Library, Version=1.0.0.0, Culture=neutral, PublicKeyToken=eb9f205c2f6f15be, ProcessorArchitecture=MSIL
Source: .\CheckDefaultLangEvent\bin\Release\CheckDefaultLangEvent.dll; DestDir: {app}; Flags: gacinstall overwritereadonly ignoreversion; StrongAssemblyName: CheckDefaultLangEvent, Version=1.0.0.0, Culture=neutral, PublicKeyToken=eb9f205c2f6f15be, ProcessorArchitecture=MSIL
Source: .\CreateGUIDEvent\bin\Release\CreateGUIDEvent.dll; DestDir: {app}; Flags: gacinstall overwritereadonly ignoreversion; StrongAssemblyName: CreateGUIDEvent, Version=1.0.0.0, Culture=neutral, PublicKeyToken=eb9f205c2f6f15be, ProcessorArchitecture=MSIL
Source: .\ListsInstallation\bin\Release\ListsInstallation.dll; DestDir: {app}; Flags: gacinstall overwritereadonly ignoreversion; StrongAssemblyName: ListsInstallation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=eb9f205c2f6f15be, ProcessorArchitecture=MSIL
Source: .\ReloadCacheEvent\bin\Release\ReloadCacheEvent.dll; DestDir: {app}; Flags: gacinstall overwritereadonly ignoreversion; StrongAssemblyName: ReloadCacheEvent, Version=1.0.0.0, Culture=neutral, PublicKeyToken=eb9f205c2f6f15be, ProcessorArchitecture=MSIL
Source: .\Alphamosaik.Oceanik.AutomaticTranslation\bin\Release\Alphamosaik.Oceanik.AutomaticTranslation.dll; DestDir: {app}; Flags: gacinstall overwritereadonly ignoreversion; StrongAssemblyName: Alphamosaik.Oceanik.AutomaticTranslation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=eb9f205c2f6f15be, ProcessorArchitecture=MSIL
Source: .\Alphamosaik.Oceanik.Sdk\bin\Release\Alphamosaik.Oceanik.Sdk.dll; DestDir: {app}; Flags: gacinstall overwritereadonly ignoreversion; StrongAssemblyName: Alphamosaik.Oceanik.Sdk, Version=1.0.0.0, Culture=neutral, PublicKeyToken=eb9f205c2f6f15be, ProcessorArchitecture=MSIL
Source: .\Translator2009Installer\bin\Release\Translator2009Installer.exe; DestDir: {app}; Flags: overwritereadonly ignoreversion
Source: .\ApplicationFeatures\bin\Release\Alphamosaik.Translator.ApplicationFeatures.wsp; DestDir: {app}; Flags: overwritereadonly ignoreversion
Source: .\Oceanik.CQWP.WebPart\Oceanik.CQWP.WebPart\bin\Release\Oceanik.CQWP.WebPart.wsp; DestDir: {app}; Flags: overwritereadonly ignoreversion
Source: .\Extractor\Extractor\Extractor\bin\Release\TranslationsExtractor.exe; DestDir: {app}; Flags: overwritereadonly ignoreversion
Source: .\InstallHelperConsole\bin\Release\InstallHelperConsole.exe; DestDir: {app}; AfterInstall: InstallWsp; Flags: overwritereadonly ignoreversion
Source: ReleaseNotes.txt; DestDir: {app}; Flags: overwritereadonly ignoreversion

[Dirs]
Name: {cf}\Microsoft Shared\web server extensions\14\TEMPLATE\Images
Name: {cf}\Microsoft Shared\web server extensions\14\TEMPLATE\LAYOUTS
Name: {app}\logs; Permissions: everyone-modify


[Icons]
Name: {group}\Activate Oceanik on a Web Application; Filename: {app}\Translator2009Installer.exe
Name: {group}\Help and Documentation; Filename: {app}\Help Oceanik 2010.pdf
Name: {group}\SDK; Filename: {app}\SDK - Sharepoint Translator.chm
Name: {group}\{cm:UninstallProgram, Oceanik}; Filename: {uninstallexe}


[UninstallDelete]

[Run]
Filename: {app}\Translator2009Installer.exe; WorkingDir: {app}; Description: Activate Oceanik on a Web Application; StatusMsg: Activating Oceanik on a Web Application; Flags: nowait postinstall


[Code]

//*********************************************************************************
// This is where all starts.
//*********************************************************************************

{*** INITIALISATION ***}
Procedure InitializeWizard;
var
VersionLabel: TNewStaticText;

begin
	VersionLabel := TNewStaticText.Create(WizardForm);
	VersionLabel.Caption := 'Version {#OCEANIK_VERSION}';
	VersionLabel.Cursor := crHand;
	VersionLabel.Parent := WizardForm;
	{ Alter Font *after* setting Parent so the correct defaults are inherited first }
	VersionLabel.Font.Color := clBlack;
	VersionLabel.Top := WizardForm.ClientHeight - VersionLabel.Height - 15;
	VersionLabel.Left := ScaleX(20);
end;

function InitializeSetup(): Boolean;
begin
	//init windows version
	initwinversion();

	msi31('3.1');

	dotnetfx35sp1();

	Result := true;
end;

procedure InstallWsp();
var
 ResultCode: Integer;
begin
	Exec(ExpandConstant('{app}\InstallHelperConsole.exe'), 'install', ExpandConstant('{app}'), SW_HIDE, ewWaitUntilTerminated, ResultCode);

	 if ResultCode <> 0 then
		begin
			RaiseException('InstallWsp failed to install properly.');
		end
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
	ResultCode: Integer;
begin
  case CurUninstallStep of
    usUninstall:
      begin
		ResultCode := 1
		if ResultCode <> 0 then
			begin
				Exec(ExpandConstant('{app}\InstallHelperConsole.exe'), 'uninstall', ExpandConstant('{app}'), SW_HIDE, ewWaitUntilTerminated, ResultCode);
			end
      end;
  end;
end;
