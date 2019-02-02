function LoadSharePointPowerShellEnvironment
{
	write-host 
	write-host "Setting up PowerShell environment for SharePoint" -foregroundcolor Yellow
	write-host 
	Add-PSSnapin "Microsoft.SharePoint.PowerShell" -ErrorAction SilentlyContinue
	write-host "SharePoint PowerShell Snapin loaded." -foregroundcolor Green
}

#
# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
# Provision create service app & start service app instance
# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
#

write-host 
LoadSharePointPowerShellEnvironment

write-host 
write-host "[[STEP]] Creating Oceanik Service Application." -foregroundcolor Yellow
write-host 

write-host "Ensure service application not already created..." -foregroundcolor Gray
$serviceApp = Get-SPServiceApplication | where { $_.GetType().FullName -eq "Alphamosaik.Oceanik.ApplicationService.OceanikServiceApplication" -and $_.Name -eq "Oceanik Service Application" }
if ($serviceApp -eq $null){
	write-host "Creating service application..." -foregroundcolor Gray
	$guid = [Guid]::NewGuid()
	$serviceApp = New-OceanikServiceApplication -Name "Oceanik Service Application" -ApplicationPool "SharePoint Web Services System"
    if ($serviceApp -ne $null){
	    write-host "Oceanik Service Application created." -foregroundcolor Green
	}
}


# [[[[[[[[STEP]]]]]]]]


write-host 
write-host "[[STEP]] Configuring permissions on Oceanik Service Application." -foregroundcolor Yellow
write-host 

write-host "Configure permissions on the service app..." -foregroundcolor Gray
$user = $env:userdomain + '\' + $env:username

write-host "  Creating new claim for $user..." -foregroundcolor Gray
$userClaim = New-SPClaimsPrincipal -Identity $user -IdentityType WindowsSamAccountName
$security = Get-SPServiceApplicationSecurity $serviceApp

write-host "  Granting $user 'FULL CONTROL' to service application..." -foregroundcolor Gray
Grant-SPObjectSecurity $security $userClaim -Rights "Full Control"
Set-SPServiceApplicationSecurity $serviceApp $security

write-host "Oceanik Service Application permissions set." -foregroundcolor Green

# [[[[[[[[STEP]]]]]]]]

write-host 
write-host "[[STEP]] Starting Oceanik Service Application instance on local server." -foregroundcolor Yellow
write-host 

write-host "Ensure service instance is running on server $env:computername..." -foregroundcolor Gray
$localServiceInstance = Get-SPServiceInstance -Server $env:computername | where { $_.GetType().FullName -eq "Alphamosaik.Oceanik.ApplicationService.OceanikServiceInstance" -and $_.Name -eq "" }
if ($localServiceInstance.Status -ne 'Online'){
	write-host "Starting service instance on server $env:computername..." -foregroundcolor Gray
	Start-SPServiceInstance $localServiceInstance
	write-host "Oceanik Service Application instance started." -foregroundcolor Green
}



write-host "[[[[ Oceanik Service Application provisioned & instance started. ]]]]" -foregroundcolor Green

write-host 