#Requires -Version 5.0

<#
.SYNOPSIS
    Creates NuGet package

.PARAMETER sourceDir
    Directory with sln file

.PARAMETER packageVersion
    Nuget package version

.PARAMETER nugetExe
    Path to nuget.exe file

.PARAMETER nugetOutput
    Path to directory where NuGet packages will be created
#>

[CmdletBinding()]
param(
    [string]$sourceDir,
    [string]$packageVersion,
    [string]$nugetExe,
    [string]$nugetOutput
)

Write-Verbose "Packing NuGet"
dotnet pack $sourceDir --output $nugetOutput -p:PackageVersion=$packageVersion --include-symbols

if ($LASTEXITCODE -ne 0) {
    Throw "An error occured while packing NuGet."
}