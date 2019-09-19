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

Write-Host "Packing NuGet using paket to $nugetOutput"

& $paketExe pack $nugetOutput --include-referenced-projects --symbols

if ($LASTEXITCODE -ne 0) {
    Throw "An error occured while packing NuGet."
}