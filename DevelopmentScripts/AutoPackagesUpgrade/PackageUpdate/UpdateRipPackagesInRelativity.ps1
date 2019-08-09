<#
.SYNOPSIS

.DESCRIPTION

.EXAMPLE
#>

[CmdletBinding()]
Param(
	[Parameter(Mandatory=$True)]
	[string]$ToVersion,
	[Parameter(Mandatory=$True)]
	[string]$RelativitySourceCodePath
)
Begin
{
    . ".\Config.ps1" 
	. ".\Utils.ps1"  
}
Process
{
	Write-Verbose "Beginning of UpdateRipPackagesInRelativity.ps1"

	$packagesConfigPath = Join-Path $RelativitySourceCodePath "kCura\DevelopmentScripts\NuGet\LibraryApplications\packages.config"

	$ripPackageRowSegment = '<package id="kCura.IntegrationPoints" version="'
	$packages = Get-Content -Path $packagesConfigPath
	$oldVersion = GetCurrentRipVersionInRelativity -PackagesConfigAsText $packages -RipPackageRowSegment $ripPackageRowSegment

	Write-Host "Updating RIP packages in packages.config from $oldVersion to $ToVersion..." -ForegroundColor Cyan

	try
	{
		$oldRipVersionRow = $ripPackageRowSegment + $oldVersion
		$newRipVersionRow = $ripPackageRowSegment + $ToVersion
		$packages.replace($oldRipVersionRow, $newRipVersionRow) | Set-Content $packagesConfigPath
	}
	catch
	{
		Write-Error "Replacing RIP version in packages.config failed with $($_.Exception.Message)" -ErrorAction Stop
	}

	try 
	{
		git -C $RelativitySourceCodePath add $packagesConfigPath
	}
	catch
	{
		Write-Error "Staging packages.config failed with $($_.Exception.Message)" -ErrorAction Stop
	}

	Write-Host "RIP version in Relativity updated successfully in packages.config from $oldVersion to $ToVersion" -ForegroundColor Cyan
	
	Write-Verbose "End of UpdateRipPackagesInRelativity.ps1"
}