<#  
.SYNOPSIS  
    Updates Integration Points packages in Relativity

.PARAMETER ToVersion
    Version of Integration Points to be updated within Relativity 

.PARAMETER RelativitySourceCodePath
    Path of Relativity source code
#>

function Update-RipPackagesInRelativity
{
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)]
        [string]
        $ToVersion,

        [Parameter(Mandatory=$True)]
        [string]
        $RelativitySourceCodePath
    )

    Import-Module -Name $PSScriptRoot\..\Import-Config
    Import-Module -Name $PSScriptRoot\..\Import-Utils 

    Write-Host "Beginning of $($MyInvocation.MyCommand.Name)"

    $packagesConfigPath = Join-Path $RelativitySourceCodePath "kCura\DevelopmentScripts\NuGet\LibraryApplications\packages.config"

    $ripPackageRowSegment = '<package id="kCura.IntegrationPoints" version="'
    $packages = Get-Content -Path $packagesConfigPath
    $oldVersion = Find-CurrentRipVersionInRelativity -PackagesConfigAsText $packages -RipPackageRowSegment $ripPackageRowSegment

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
    
    Write-Host "End of $($MyInvocation.MyCommand.Name)"
}

Export-ModuleMember -Function Update-RipPackagesInRelativity