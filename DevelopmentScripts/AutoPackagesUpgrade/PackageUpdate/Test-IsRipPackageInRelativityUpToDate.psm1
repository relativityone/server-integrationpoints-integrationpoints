<#  
.SYNOPSIS  
    Checks if Integration Points package is up to date within Relativity 

.PARAMETER NewVersion
    New version of Integration Points package

.PARAMETER RipSourceCodePath
    Path of Relativity source code
#>

function Test-IsRipPackageInRelativityUpToDate
{
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)]
        [string]
        $NewVersion,

        [Parameter(Mandatory=$True)]
        [string]
        $RelativitySourceCodePath
    )

    Import-Module -Name $PSScriptRoot\..\Import-Utils

    Write-Host "Beginning of $($MyInvocation.MyCommand.Name)"

    $packagesConfigPath = Join-Path $RelativitySourceCodePath "kCura\DevelopmentScripts\NuGet\LibraryApplications\packages.config"
    $ripPackageRowSegment = '<package id="kCura.IntegrationPoints" version="'
    $packages = Get-Content -Path $packagesConfigPath
    $oldVersion = Find-CurrentRipVersionInRelativity -PackagesConfigAsText $packages -RipPackageRowSegment $ripPackageRowSegment

    $newSystemVersion = Format-RipPackageVersionToSystemVersion -PackageVersion $NewVersion
    $oldSystemVersion = Format-RipPackageVersionToSystemVersion -PackageVersion $oldVersion

    $isUpToDate = $newSystemVersion -le $oldSystemVersion

    if($isUpToDate -eq $true)
    {
        Write-Host "RIP version in Relativity is up to date." -ForegroundColor Cyan
    }
    
    Write-Host "End of $($MyInvocation.MyCommand.Name)"
    
    $isUpToDate
}

Export-ModuleMember -Function Test-IsRipPackageInRelativityUpToDate