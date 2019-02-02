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
# Provision create service app proxy & add proxy to defalut proxy group
# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
#

write-host 
LoadSharePointPowerShellEnvironment

# [[[[[[[[STEP]]]]]]]]

write-host "Get reference to Oceanik Service Application" -foregroundcolor Gray
$serviceApp = Get-SPServiceApplication | where { $_.GetType().FullName -eq "Alphamosaik.Oceanik.ApplicationService.OceanikServiceApplication" -and $_.Name -eq "Oceanik Service Application" }
if ($serviceApp -eq $null){
	Write-Error "CRITICAL ERROR: Failed to acquire reference to Oceanik Service Application!!!!"
}

write-host 
write-host "[[STEP]] Creating Oceanik Service Application proxy." -foregroundcolor Yellow
write-host 

write-host "Ensure service application proxy not already created..." -foregroundcolor Gray
$serviceAppProxy = Get-SPServiceApplicationProxy | where { $_.GetType().FullName -eq "Alphamosaik.Oceanik.ApplicationService.OceanikServiceApplication" -and $_.Name -eq "Oceanik Service Application Proxy" }
if ($serviceAppProxy -eq $null)
{
	write-host "Creating service application proxy..." -foregroundcolor Gray
	$serviceAppProxy = New-OceanikServiceApplicationProxy -Name "Oceanik Service Application Proxy" -ServiceApplication $serviceApp
	write-host "Oceanik Service Application proxy created." -foregroundcolor Green

	write-host 

	write-host "Adding service application proxy to default group..." -foregroundcolor Gray
	Get-SPServiceApplicationProxyGroup -Default | Add-SPServiceApplicationProxyGroupMember -Member $serviceAppProxy
	write-host "Oceanik Service Application added to default group." -foregroundcolor Green
}



write-host "[[[[ Oceanik Service Application components setup. ]]]]" -foregroundcolor Green