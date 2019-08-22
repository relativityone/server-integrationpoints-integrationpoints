<#
.SYNOPSIS

.DESCRIPTION

.EXAMPLE
#>

[CmdletBinding()]
Param(
	[Parameter(Mandatory=$True)]
    [string]$NewVersion,
	[Parameter(Mandatory=$True)]
    [string]$RelativitySourceCodePath
)
Begin
{
    . ".\Utils.ps1" 
}
Process
{
	Write-Verbose "Beginning of IsRipPackageInRelativityUpToDate.ps1"

	$packagesConfigPath = Join-Path $RelativitySourceCodePath "kCura\DevelopmentScripts\NuGet\LibraryApplications\packages.config"
	$ripPackageRowSegment = '<package id="kCura.IntegrationPoints" version="'
	$packages = Get-Content -Path $packagesConfigPath
	$oldVersion = GetCurrentRipVersionInRelativity -PackagesConfigAsText $packages -RipPackageRowSegment $ripPackageRowSegment

    $newSystemVersion = MapRipPackageVersionToSystemVersion -PackageVersion $NewVersion
    $oldSystemVersion = MapRipPackageVersionToSystemVersion -PackageVersion $oldVersion

    $isUpToDate = $newSystemVersion -le $oldSystemVersion

    if($isUpToDate -eq $true)
    {
        Write-Host "RIP version in Relativity is up to date." -ForegroundColor Cyan
    }
	
    Write-Verbose "End of IsRipPackageInRelativityUpToDate.ps1"
    
    return $isUpToDate
}