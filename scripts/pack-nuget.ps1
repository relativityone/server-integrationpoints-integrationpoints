#Requires -Version 5.0

<#
.SYNOPSIS
    Creates NuGet package

.PARAMETER packageVersion
    Nuget package version

.PARAMETER paketExe
    Path to paket.exe file

.PARAMETER nugetOutput
    Path to directory where NuGet packages will be created
#>

[CmdletBinding()]
param(
    [string]$packageVersion,
    [string]$paketExe,
    [string]$nugetOutput
)

Write-Verbose "Packing NuGet using paket..."
& $paketExe pack $nugetOutput --include-referenced-projects --version $packageVersion --symbols

if ($LASTEXITCODE -ne 0) {
    Throw "An error occured while packing NuGet."
}