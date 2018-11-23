#Requires -Version 5.0

<#
.SYNOPSIS
    Downloads nuget.exe if missing and restores buildtools dependencies

.PARAMETER nugetExe
    Path to nuget.exe file

.PARAMETER toolsDir
    buildtools directory
#>

[CmdletBinding()]
param(
    [string]$nugetExe,
    [string]$toolsDir
)

$nugetUrl = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
$jetBrainsUrl = "https://resharper-plugins.jetbrains.com/api/v2"
$toolsPackagesFile = Join-Path $toolsDir "packages.config"
$jetBrainsSourceName = "JetBrains"

Write-Verbose "Checking for NuGet in buildtools path..."
if (-Not (Test-Path $nugetExe)) {
    Write-Output "Installing NuGet from $nugetUrl..."
    Write-Verbose "To $nugetExe..."
    Invoke-WebRequest $nugetUrl -OutFile $nugetExe -ErrorAction Stop
}

Write-Verbose "Checking if JetBrains is added to NuGet sources..."
$isJetBrainsDefined = (& $nugetExe source list | Select-String $jetBrainsSourceName)
if ($isJetBrainsDefined) {
    Write-Verbose "Updating JetBrains source to NuGet..."
    & $nugetExe sources update -Name "JetBrains" -Source $jetBrainsUrl -Verbosity quiet
}
else {
    Write-Output "Adding JetBrains source to NuGet..."
    & $nugetExe sources add -Name "JetBrains" -Source $jetBrainsUrl -Verbosity quiet
}

if ($LASTEXITCODE -ne 0) {
    Throw "An error occured while adding JetBrains source to NuGet."
}

Write-Output "Restoring tools from NuGet..."
Write-Verbose "Using $toolsPackagesFile..."
& $nugetExe install $toolsPackagesFile -o $toolsDir > $null

if ($LASTEXITCODE -ne 0) {
    Throw "An error occured while restoring tools from NuGet."
}