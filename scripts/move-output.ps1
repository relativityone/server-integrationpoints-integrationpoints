#Requires -Version 5.0

<#
.SYNOPSIS
    Prepare for Paket package creation

.PARAMETER Source
    Current project output path

.PARAMETER Destination
    New project output path
#>

[CmdletBinding()]
param(
    [string]$Source,
    [string]$Destination
)


Write-Host "Preparing for packing by moving project output to another directory because of paket requirements"

$childItems = Get-ChildItem $Source

if(-not (Test-Path $Destination)) {
    New-Item $Destination -ItemType Directory
}

$childItems | ForEach-Object {
    Copy-Item -Path $_.FullName -Destination $Destination -Recurse -Force
}

if ($LASTEXITCODE -ne 0) {
    Throw "An error occured while moving project output to another folder."
}

