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
	Write-Verbose "Beginning of Is-RipPackageInRelativityUpToDate.ps1"

	$packagesConfigPath = Join-Path $RelativitySourceCodePath "kCura\DevelopmentScripts\NuGet\LibraryApplications\packages.config"
	$ripPackageRowSegment = '<package id="kCura.IntegrationPoints" version="'
	$packages = Get-Content -Path $packagesConfigPath
	$oldVersion = Get-CurrentRipVersionInRelativity -PackagesConfigAsText $packages -RipPackageRowSegment $ripPackageRowSegment

    $newSystemVersion = Map-RipPackageVersionToSystemVersion -PackageVersion $NewVersion
    $oldSystemVersion = Map-RipPackageVersionToSystemVersion -PackageVersion $oldVersion

    $isUpToDate = $newSystemVersion -le $oldSystemVersion

    if($isUpToDate -eq $true)
    {
        Write-Host "RIP version in Relativity is up to date." -ForegroundColor Cyan
    }
	
    Write-Verbose "End of Is-RipPackageInRelativityUpToDate.ps1"
    
    $isUpToDate
}