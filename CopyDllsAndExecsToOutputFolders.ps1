# Ce script copie les DLLs et Exe sous \Output\(Debug | Release)
# Pour faciliter les tests de deverminages
   
# --------------------------
# Le main commence ici      
# --------------------------  

$Dir = get-childitem -recurse
$ListDlls = $Dir | where {$_.extension -eq ".dll"}
$ListExecs = $Dir | where {$_.extension -eq ".exe"}

$actualPath = (Resolve-Path "..\")
$outputPathDebug = $actualPath.path
$outputPathDebug += "\Output\Debug"

$outputPathRelease = $actualPath.path
$outputPathRelease += "\Output\Release"

[IO.Directory]::CreateDirectory($outputPathDebug) 
[IO.Directory]::CreateDirectory($outputPathRelease) 

foreach($filename in $ListDlls)
{
    if($filename.DirectoryName.Contains('bin\Debug'))
    {
        $fullPath = $filename.FullName
        Copy-Item $fullPath $outputPathDebug -Force
    }    
    
    if($filename.DirectoryName.Contains('bin\Release'))
    {
        $fullPath = $filename.FullName
        Copy-Item $fullPath $outputPathRelease -Force
    }        
}

foreach($filename in $ListExecs)
{
    if($filename.DirectoryName.Contains('bin\Debug'))
    {
        $fullPath = $filename.FullName
        Copy-Item $fullPath $outputPathDebug -Force
    }    
    
    if($filename.DirectoryName.Contains('bin\Release'))
    {
        $fullPath = $filename.FullName
        Copy-Item $fullPath $outputPathRelease -Force
    }        
}