<#  
.SYNOPSIS  
    Checks if package is up to date considering specified version 

.PARAMETER PackageName
    Package name

.PARAMETER NewVersion
    New version of package

.PARAMETER RipSourceCodePath
    Path of Integration Points source code
#>

function Test-IsPackageInRipUpToDate
{
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)]
        [string]
        $PackageName,

        [Parameter(Mandatory=$True)]
        [string]
        $NewVersion,

        [Parameter(Mandatory=$True)]
        [string]
        $RipSourceCodePath
    )

    Import-Module -Name $PSScriptRoot\..\Import-Config
    Import-Module -Name $PSScriptRoot\..\Import-Utils    

    Write-Host "Beginning of $($MyInvocation.MyCommand.Name)"

    $paketDependenciesPath = Join-Path $RipSourceCodePath "paket.dependencies"
    
    $packages = Get-Content -Path $paketDependenciesPath
    $oldVersion = Find-CurrentPackageVersionInRip -PaketDependenciesAsText $packages -PackageName $PackageName

    Write-Host "Comparing versions [$NewVersion] to [$oldVersion] of $PackageName"

    $newSystemVersion = Format-RelativityPackageVersionToSystemVersion -PackageVersion $NewVersion
    $oldSystemVersion = Format-RelativityPackageVersionToSystemVersion -PackageVersion $oldVersion

    $isUpToDate = $newSystemVersion -le $oldSystemVersion

    if($isUpToDate -eq $true)
    {
        Write-Host "$PackageName version ($oldVersion) in RIP is up to date ($NewVersion)." -ForegroundColor Cyan
    }
    
    Write-Host "End of $($MyInvocation.MyCommand.Name)"
    
    $isUpToDate
}

Export-ModuleMember -Function Test-IsPackageInRipUpToDate