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
	[string]$OnBranch,
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

    $stashName = "Saved changes before checking packages"
    $initialBranchName = .\Git\GetCurrentBranchName.ps1 -Path $RelativitySourceCodePath

    .\Git\Stash.ps1 -StashName $stashName -Path $RelativitySourceCodePath

    .\Git\Checkout.ps1 -BranchName $OnBranch -Path $RelativitySourceCodePath

	$packagesConfigPath = Join-Path $RelativitySourceCodePath "kCura\DevelopmentScripts\NuGet\LibraryApplications\packages.config"
	
	$ripPackageRowSegment = '<package id="kCura.IntegrationPoints" version="'
	$packages = Get-Content -Path $packagesConfigPath
	$oldVersion = GetCurrentRipVersionInRelativity -PackagesConfigAsText $packages -RipPackageRowSegment $ripPackageRowSegment

    .\Git\Checkout.ps1 -BranchName $initialBranchName -Path $RelativitySourceCodePath

	.\Git\PopStashIfExistsOnTop.ps1 -StashName $stashName -Path $RelativitySourceCodePath

    $newSystemVersion = MapRipPackageVersionToSystemVersion -PackageVersion $NewVersion
    $oldSystemVersion = MapRipPackageVersionToSystemVersion -PackageVersion $oldVersion

    $isUpToDate = $newSystemVersion -le $oldSystemVersion

    if($isUpToDate)
    {
        Write-Host "RIP version in Relativity is up to date." -ForegroundColor Cyan
    }
	
    Write-Verbose "End of IsRipPackageInRelativityUpToDate.ps1"
    
    return $isUpToDate
}