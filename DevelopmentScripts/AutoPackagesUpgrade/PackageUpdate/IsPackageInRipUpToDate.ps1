<#
.SYNOPSIS

.DESCRIPTION

.EXAMPLE
#>

[CmdletBinding()]
Param(
    [Parameter(Mandatory=$True)]
    [string]$PackageName,
	[Parameter(Mandatory=$True)]
    [string]$NewVersion,
	[Parameter(Mandatory=$True)]
    [string]$RipSourceCodePath
)
Begin
{
    . ".\Config.ps1" 
	. ".\Utils.ps1"      
}
Process
{
	Write-Verbose "Beginning of IsPackageInRipUpToDate.ps1"

	$paketDependenciesPath = Join-Path $RipSourceCodePath "paket.dependencies"
	
	$packages = Get-Content -Path $paketDependenciesPath
	$oldVersion = GetCurrentPackageVersionInRip -PaketDependenciesAsText $packages -PackageName $PackageName

    $newSystemVersion = MapRelativityPackageVersionToSystemVersion -PackageVersion $NewVersion
    $oldSystemVersion = MapRelativityPackageVersionToSystemVersion -PackageVersion $oldVersion

    $isUpToDate = $newSystemVersion -le $oldSystemVersion

    if($isUpToDate -eq $true)
    {
        Write-Host "$PackageName version ($oldVersion) in RIP is up to date ($NewVersion)." -ForegroundColor Cyan
    }
	
    Write-Verbose "End of IsPackageInRipUpToDate.ps1"
    
    return $isUpToDate
}