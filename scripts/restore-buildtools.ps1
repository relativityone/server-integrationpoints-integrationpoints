#Requires -Version 5.0

<#
.SYNOPSIS
    Downloads nuget.exe if missing and restores buildtools dependencies

.PARAMETER nugetExe
    Where nuget.exe file should be located

.PARAMETER toolsDir
    buildtools directory
#>

[CmdletBinding()]
param(
    [string]$nugetExe,
    [string]$toolsDir
)

$nugetUrl = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
$toolsPackagesFile = Join-Path $toolsDir "packages.config"

Write-Verbose "Checking for NuGet in buildtools path..."
if (-Not (Test-Path $nugetExe)) {
    Write-Output "Installing NuGet from $nugetUrl..."
    Write-Verbose "To $nugetExe..."
    Invoke-WebRequest $nugetUrl -OutFile $nugetExe -ErrorAction Stop
}

Write-Output "Restoring tools from NuGet..."
Write-Verbose "Using $toolsPackagesFile..."
& $nugetExe install $toolsPackagesFile -o $toolsDir > $null

if ($LASTEXITCODE -ne 0) {
    Throw "An error occured while restoring tools from NuGet."
}