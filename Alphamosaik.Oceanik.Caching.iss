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

[Setup]
OutputDir=.\OceanikCaching
VersionInfoVersion=2.0.0
VersionInfoCompany=ALPHAMOSAIK
VersionInfoDescription=Oceanik Caching
VersionInfoTextVersion=2.0.0
VersionInfoCopyright=Alphamosaik 2011
SetupLogging=true
VersionInfoProductName=Oceanik Caching
VersionInfoProductVersion=1.0
MinVersion=0,5.02.3790
OnlyBelowVersion=0,0
AppName=OceanikCaching
AppVerName=Oceanik Caching 2011
DefaultDirName={pf}\Alphamosaik\OceanikCaching
DefaultGroupName=Oceanik Caching
WizardImageFile=.\MM-install.bmp
WizardSmallImageFile=.\MM-install-MSI_mini.bmp
AppCopyright=WARNING: This computer program is protected by copyright law and international treaties. Unauthorized duplication or distribution of this program, or any portion of it, may result in severe civil or criminal penalties, and will be prosecuted to the maximum extent possible under the law.
InfoBeforeFile=.\SharepointTranslator2009\WarningBeforeInstallation.rtf
LicenseFile=.\SharepointTranslator2009\NDA.rtf
ArchitecturesAllowed=x86 x64 ia64
ArchitecturesInstallIn64BitMode=x64 ia64
PrivilegesRequired=admin
AppID=Oceanik Caching


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
Source: .\InstallHelper\bin\Release\InstallHelper.dll; DestDir: {app}; Flags: overwritereadonly ignoreversion
Source: .\SharepointTranslator2009\NDA.rtf; DestDir: {app}; Flags: overwritereadonly ignoreversion
Source: .\SharepointTranslator2009\WarningBeforeInstallation.rtf; DestDir: {app}; Flags: overwritereadonly ignoreversion
Source: ..\..\..\..\Common\Main\source\Alphamosaik.Common.Library\Alphamosaik.Common.Library\bin\Release\Alphamosaik.Common.Library.dll; DestDir: {app}; Flags: gacinstall overwritereadonly ignoreversion; StrongAssemblyName: Alphamosaik.Common.Library, Version=1.0.0.0, Culture=neutral, PublicKeyToken=eb9f205c2f6f15be, ProcessorArchitecture=MSIL
Source: ..\..\..\..\Common\Main\source\Alphamosaik.Common.SharePoint.Library\bin\Release\Alphamosaik.Common.SharePoint.Library.dll; DestDir: {app}; Flags: gacinstall overwritereadonly ignoreversion; StrongAssemblyName: Alphamosaik.Common.SharePoint.Library, Version=1.0.0.0, Culture=neutral, PublicKeyToken=eb9f205c2f6f15be, ProcessorArchitecture=MSIL
Source: .\Alphamosaik.Oceanik.Caching\bin\Release\Alphamosaik.Oceanik.Caching.wsp; DestDir: {app}; Flags: overwritereadonly ignoreversion
Source: .\InstallOceanikCachingHelper\bin\Release\InstallOceanikCachingHelper.exe; DestDir: {app}; AfterInstall: InstallWsp; Flags: overwritereadonly ignoreversion
Source: CachingReleaseNotes.txt; DestDir: {app}; Flags: overwritereadonly ignoreversion

[Dirs]
Name: {cf}\Microsoft Shared\web server extensions\14\TEMPLATE\Images
Name: {cf}\Microsoft Shared\web server extensions\14\TEMPLATE\LAYOUTS
Name: {app}\logs; Permissions: everyone-modify


[Icons]
Name: {group}\{cm:UninstallProgram, Oceanik Caching}; Filename: {uninstallexe}


[UninstallDelete]

[Run]

[Code]
//*********************************************************************************
// This is where all starts.
//*********************************************************************************
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
	Exec(ExpandConstant('{app}\InstallOceanikCachingHelper.exe'), 'install', ExpandConstant('{app}'), SW_HIDE, ewWaitUntilTerminated, ResultCode);

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
				Exec(ExpandConstant('{app}\InstallOceanikCachingHelper.exe'), 'uninstall', ExpandConstant('{app}'), SW_HIDE, ewWaitUntilTerminated, ResultCode);
			end
      end;
  end;
end;
