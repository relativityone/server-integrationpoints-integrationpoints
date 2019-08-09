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
    [string]$RipSourceCodePath
)
Begin
{
    . ".\Config.ps1" 
	. ".\Utils.ps1"      
}
Process
{
	Write-Verbose "Beginning of AreRelativityPackagesInRipUpToDate.ps1"

    $stashName = "Saved changes before checking packages"
    $initialBranchName = .\Git\GetCurrentBranchName.ps1 -Path $RipSourceCodePath

    .\Git\Stash.ps1 -StashName $stashName -Path $RipSourceCodePath

    .\Git\Checkout.ps1 -BranchName $OnBranch -Path $RipSourceCodePath

	$paketDependenciesPath = Join-Path $RipSourceCodePath "paket.dependencies"
	
	$packages = Get-Content -Path $paketDependenciesPath
	$oldVersion = GetCurrentRelativityVersionInRip -PaketDependenciesAsText $packages

    .\Git\Checkout.ps1 -BranchName $initialBranchName -Path $RipSourceCodePath

	.\Git\PopStashIfExistsOnTop.ps1 -StashName $stashName -Path $RipSourceCodePath

    $newSystemVersion = MapRelativityPackageVersionToSystemVersion -PackageVersion $NewVersion
    $oldSystemVersion = MapRelativityPackageVersionToSystemVersion -PackageVersion $oldVersion

    $isUpToDate = $newSystemVersion -le $oldSystemVersion

    if($isUpToDate)
    {
        Write-Host "Relativity version in RIP is up to date." -ForegroundColor Cyan
    }
	
    Write-Verbose "End of AreRelativityPackagesInRipUpToDate.ps1"
    
    return $isUpToDate
}