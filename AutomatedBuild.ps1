$commandExecTFS = """C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\tf.exe"""
$commandExecStudio2010 = """C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\devenv.exe"""
$commandExecISTools = "..\..\..\..\Common\Main\tools\ISTool\ISTool.exe"
$commandExecDotNetReactor = """C:\Program Files (x86)\Eziriz\.NET Reactor\dotNET_Reactor.Console.exe"""

$global:tfsUsernamePassword = ""
$global:fullVersion = ""

function global:BuildSucceeded
{
    " ______        _ _     _     ______                             _           _    _ _ _ "
    "(____  \      (_) |   | |   / _____)                           | |         | |  | | | |"
    " ____)  )_   _ _| | __| |  ( (____  _   _  ____  ____ _____  __| |_____  __| |  | | | |"
    "|  __  (| | | | | |/ _  |   \____ \| | | |/ ___)/ ___) ___ |/ _  | ___ |/ _  |  |_|_|_|"    
    "| |__)  ) |_| | | ( (_| |   _____) ) |_| ( (___( (___| ____( (_| | ____( (_| |   _ _ _ "
    "|______/|____/|_|\_)____|  (______/|____/ \____)\____)_____)\____|_____)\____|  |_|_|_|"
}

function global:BuildFailed
{
    "########  ##     ## #### ##       ########     ########    ###    #### ##       ######## ########     #### #### ####"
    "##     ## ##     ##  ##  ##       ##     ##    ##         ## ##    ##  ##       ##       ##     ##    #### #### ####"
    "##     ## ##     ##  ##  ##       ##     ##    ##        ##   ##   ##  ##       ##       ##     ##    #### #### ####"
    "########  ##     ##  ##  ##       ##     ##    ######   ##     ##  ##  ##       ######   ##     ##     ##   ##   ## "
    "##     ## ##     ##  ##  ##       ##     ##    ##       #########  ##  ##       ##       ##     ##                  "
    "##     ## ##     ##  ##  ##       ##     ##    ##       ##     ##  ##  ##       ##       ##     ##    #### #### ####"
    "########   #######  #### ######## ########     ##       ##     ## #### ######## ######## ########     #### #### ####"
}

function global:EncryptAssemblies ($xmlDoc="$PWD\AutomatedBuildConfig.xml")
{    
    $xmlDoc = (Resolve-Path $xmlDoc)
    [xml]$x = get-content $xmlDoc
    
    $xmldata = [xml]$x
    
    $commandExec = $commandExecDotNetReactor
    
    $mainAssembly = (Resolve-Path $xmldata.build.mainAssembly.path)
    $commandArg += " -file "
    $commandArg += """"
    $commandArg += $mainAssembly
    $commandArg += """"
    
    $commandArg += " -satellite_assemblies "
    $commandArg += """"
    
    $firstTime = $True
    foreach ($assemblyPath in $xmldata.build.satelliteAssemblies.path)
    {
        if ($firstTime -eq $False)
        {
            $commandArg += "/"
        }
        else
        {
            $firstTime = $False
        }
        
        $fullPath = (Resolve-Path $assemblyPath)   
        $commandArg += $fullPath
    }
    $commandArg += """"
   
    $snkeypair = (Resolve-Path $xmldata.build.tools.dotNetReactor.strongNameKeyPairFile)
    $commandArg += " -snkeypair "
    $commandArg += """"
    $commandArg += $snkeypair
    $commandArg += """"
    
    $commandArg += " -targetfile ""<AssemblyLocation>\<AssemblyFileName>"""
    
    "`n`n- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - `n"
    "Encrypting DLLs: " + $commandExec + $commandArg
    
    start-process -wait $commandExec $commandArg
    if ($? -eq $false)
    {
        "DotNetReactor Error: Could not encrypt files: " + $LastExitCode
        return $false
    }
}

function global:BuildSolution ($xmlDoc="$PWD\AutomatedBuildConfig.xml")
{   
    # Va chercher la dernière révision des fichiers
    $commandExec = $commandExecTFS

    $xmlDoc = (Resolve-Path $xmlDoc)
    [xml]$x = get-content $xmlDoc
   
    $xmldata = [xml]$x
    
    $branchName = $xmldata.build.branch
    
    $commandArg = " get """
    $commandArg += $branchName
    $commandArg += """ /login:"
    $commandArg += $tfsUsernamePassword

    "`n`n- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - `n"        
    #"Getting Latest files: " + $commandExec + $commandArg
    "Getting Latest files..."

    start-process -wait $commandExec $commandArg
    if ($? -eq $false)
    {
        "TFS Error: Could not do the Get Latest: " + $LastExitCode
        return $false
    }
    
    # Build des solutions
    $commandExec = $commandExecStudio2010
    $config = $xmldata.build.config
    
    foreach ($solution in $xmldata.build.solutions.solution)
    {
        $fullPath = (Resolve-Path $solution)
        
        $commandArg = """"
        $commandArg += $fullPath
        $commandArg += """ "
        
        $commandArg += "/rebuild "
        $commandArg += $config

        "`n`n- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - `n"
        "Building: " + $commandExec + $commandArg
       
        start-process -wait $commandExec $commandArg
        if ($? -eq $false)
        {
            "DevEnv Error: Could not build solution: " + $LastExitCode
            return $false
        }
    }
}

function global:CreateWSP ($xmlDoc="$PWD\AutomatedBuildConfig.xml")
{    
    $xmlDoc = (Resolve-Path $xmlDoc)
    [xml]$x = get-content $xmlDoc
    
    $xmldata = [xml]$x
    
    $commandExec = $commandExecStudio2010
    $config = $xmldata.build.config

    foreach ($package in $xmldata.build.packages.package)
    {
        $fullPath = (Resolve-Path $package)
        
        $commandArg = """"
        $commandArg += $fullPath
        $commandArg += """ "
        
        $commandArg += "/rebuild "
        $commandArg += $config

        "`n`n- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - `n"
        "Building: " + $commandExec + $commandArg
       
        start-process -wait $commandExec $commandArg
        if ($? -eq $false)
        {
            "DevEnv Error: Could not create WPS: " + $LastExitCode
            return $false
        }        
    }
}

function ConvertTo-PlainText( [security.securestring]$secure )
{
    $marshal = [Runtime.InteropServices.Marshal]
    $marshal::PtrToStringAuto( $marshal::SecureStringToBSTR($secure) )
}

function global:GetTFSUsernamePasswordString ($dummy)
{
    $tfsUsername = Read-Host "Please enter your TFS username (ex: alpha\c0123)"
    $tfsPassword = Read-Host -assecurestring "Please enter your password"
    $tfsPasswordText = ConvertTo-PlainText($tfsPassword)
    
    $tfsUsernamePassword = $tfsUsername + "," + $tfsPasswordText
    
    $tfsUsernamePassword
}

# Cette fonction lit le fichier dans une variable
# On remplace le texte (version) et sauvegarde dans un fichier .tmp
# Ensuite on deplace le .tmp dans le fichier original
# Il y a validation de la grosseur du fichier pour s'assurer 
# que le fichier ne varie que de quelques octets apres la mise a jour de la version

function global:SafeUpdateVersionInfo($fullPath, $match, $replacement)
{
    "Updating version in file: " + $fullPath

    $fullPathTemp = (get-item $fullPath).FullName
    $fullPathTemp += ".tmp"
           
    $fileBefore = get-childitem $fullPath
    $fileSizeBefore = $fileBefore.length
    
    $content = (Get-Content $fullPath)
    #$content

    $content = $content -creplace $match,$replacement
    $content | Set-Content $fullPathTemp
    #$content
    
    Move-Item $fullPathTemp $fullPath -force
    if ($? -eq $false)
    {
        "File Move failed: " + $fullPathTemp
         return $false
    }

    $fileAfter = get-childitem $fullPath
    $fileSizeAfter = $fileAfter.length
    
    if(($fileSizeAfter -lt ($fileSizeBefore - 10)) -or ($fileSizeAfter -gt ($fileSizeBefore + 10)) )
    {
        "File size changed after update. Not normal !!! : " + $fullPathTemp
        return $false
    }
    
    return $true
}

function global:UpdateBuildVersion ($xmlDoc="$PWD\AutomatedBuildConfig.xml")
{   
    $xmlDoc = (Resolve-Path $xmlDoc)
    [xml]$x = get-content $xmlDoc
    $xmldata = [xml]$x

    $commandExec = $commandExecTFS
    
    # On va lire la liste de fichier
    $fileList = ""
    foreach ($assemblyInfo in $xmldata.build.assemblyInfo.path)
    {
        $fullPath = (Resolve-Path $assemblyInfo)
        $fileList += """" + $fullPath + """ "
    }

    # Ajout du script de l'installer à la liste
    $installerScript = (Resolve-Path $xmldata.build.tools.innosetup.script)
    
    $fileList += """" 
    $fileList += $installerScript
    $fileList += """ "
    $fileList
    
    # Checkout des fichiers    
    $commandArg = " checkout  /noprompt /login:"
    $commandArg += $tfsUsernamePassword + " "
    $commandArg += $fileList
        
    "`n`n- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - `n"        
    #"Checking out: " + $commandExec + $commandArg
    "Checking out files for version update..."
    start-process -wait $commandExec $commandArg
    if ($? -eq $false)
    {
        "TFS Error: Could not checkout files Code: " + $LastExitCode
        return $false
    }
    
    # Remplace avec le nouveau numero de version    
    $version = $xmldata.build.version
    $monthDay = Get-Date -format ".MM.dd"
    $global:fullVersion = $version
    $global:fullVersion += $monthDay
    
    $match = "(\[assembly: AssemblyFileVersion\("")+(\d+\.\d+\.\d+\.\d+)+(""\)\])"
    $replacement = "[assembly: AssemblyFileVersion("""
    $replacement += $global:fullVersion
    $replacement += """)]"
    
    # Maj des AssemblyInfo
    foreach ($assemblyInfo in $xmldata.build.assemblyInfo.path)
    {
        $fullPath = (Resolve-Path $assemblyInfo)
  
        SafeUpdateVersionInfo $fullPath $match $replacement
        if($? -eq $false)
        {
            return $false
        }        
    }    
    
    # Maj des versions dans le script de l'installer
    
    # Maj de VersionInfoProductVersion
    $match = "(VersionInfoProductVersion=)(\d+\.\d+)"
    $replacement = "VersionInfoProductVersion="
    $replacement += $version
    
    SafeUpdateVersionInfo $installerScript $match $replacement
    if($? -eq $false)
    {
        return $false
    }
    
    # Maj de VersionInfoVersion
    $match = "(VersionInfoVersion=)(\d+\.\d+\.\d+\.\d+)"
    $replacement = "VersionInfoVersion="
    $replacement += $global:fullVersion
    
    SafeUpdateVersionInfo $installerScript $match $replacement
    if($? -eq $false)
    {
        return $false
    }
    
    # Maj de VersionInfoTextVersion
    $match = "(VersionInfoTextVersion=)(\d+\.\d+\.\d+\.\d+)"
    $replacement = "VersionInfoTextVersion="
    $replacement += $global:fullVersion
    
    SafeUpdateVersionInfo $installerScript $match $replacement
    if($? -eq $false)
    {
        return $false
    }                    
      
    # Maj de OCEANIK_VERSION
    $match = "(#define OCEANIK_VERSION "")(\d+\.\d+\.\d+\.\d+)("")"
    $replacement = "#define OCEANIK_VERSION """
    $replacement += $global:fullVersion
    $replacement += """"
    
    SafeUpdateVersionInfo $installerScript $match $replacement
    if($? -eq $false)
    {
        return $false
    }                    
      
    # Checkin des fichiers
    $commandArg = " checkin /comment:""Mis à jour par le processus de build pour la version "
    $commandArg += $global:fullVersion
    $commandArg += """ /noprompt /login:"    
    $commandArg += $tfsUsernamePassword + " "
    $commandArg += $fileList
       
    "`n`n- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - `n"
    #"Checking in: " + $commandExec + $commandArg
    "Checking in files for version update..."
    start-process -wait $commandExec $commandArg
    if ($? -eq $false)
    {
        "TFS Error: Could not checkin files Code: " + $LastExitCode
        return $false
    }
}      


function global:CreateInstaller ($xmlDoc="$PWD\AutomatedBuildConfig.xml")
{    
    $xmlDoc = (Resolve-Path $xmlDoc)
    [xml]$x = get-content $xmlDoc
    
    $xmldata = [xml]$x
    
    $installerExec = (Resolve-Path $commandExecISTools)
    $commandExec = """"
    $commandExec += $installerExec
    $commandExec += """"
        
    $scriptFile = (Resolve-Path $xmldata.build.tools.innosetup.script)
        
    $commandArg = "-compile "
    $commandArg += """"
    $commandArg += $scriptFile
    $commandArg += """ "

    "`n`n- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - `n"    
    "Creating Installer: " + $commandExec + $commandArg
    
    start-process -wait $commandExec $commandArg   
    if ($? -eq $false)
    {
        "Installer Error: Could not create setup file: " + $LastExitCode
        return $false
    }
    
    $setupFile = (Resolve-Path "Oceanik\Setup.exe")
    $oceanikSetupFile = "Oceanik2010 v"
    $oceanikSetupFile += $global:fullVersion
    $oceanikSetupFile += ".exe"
    
    $oldSetupToDelete = "Oceanik\"
    $oldSetupToDelete += $oceanikSetupFile
    
    $oldSetupToDeletePath = (Resolve-Path $oldSetupToDelete)

    if ($oldSetupToDeletePath)
    {
        "Deleting existing installer file"
        Remove-Item (Resolve-Path $oldSetupToDelete)
    }    
    
    Rename-Item $setupFile $oceanikSetupFile
    if ($? -eq $false)
    {
        "File rename failed: " + $setupFile
         return $false
    }   
}
    
# --------------------------
# Le main commence ici      
# --------------------------  

"`n`n- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - `n"
" Build Script Started"

# Va chercher username et password de TFS
$tfsUsernamePassword = GetTFSUsernamePasswordString ("")

# Maj des versions
UpdateBuildVersion "$PWD\AutomatedBuildConfig.xml"
if ($? -eq $false)
{
    BuildFailed
    return $false
}

# Build la solution complète
BuildSolution "$PWD\AutomatedBuildConfig.xml"
if ($? -eq $false)
{
    BuildFailed
    return $false
}

# Encryption des DLLs
EncryptAssemblies "$PWD\AutomatedBuildConfig.xml"
if ($? -eq $false)
{
    BuildFailed
    return $false
}

# Création des WSP
CreateWSP "$PWD\AutomatedBuildConfig.xml"
if ($? -eq $false)
{
    BuildFailed
    return $false
}

# Création de l'installer
CreateInstaller "$PWD\AutomatedBuildConfig.xml"
if ($? -eq $false)
{
    BuildFailed
    return $false
}

BuildSucceeded

Read-Host "Press ENTER-OK to close this Windows !"