#Requires -Version 5.0

<#
.SYNOPSIS
    Restore all dependecies using paket

.PARAMETER paketExe
    Path to paket.exe file
#>

[CmdletBinding()]
param(
    [string]$paketExe
)

& $paketExe restore

if ($LASTEXITCODE -ne 0) {
    Throw "An error occured while restoring packages using paket."
}