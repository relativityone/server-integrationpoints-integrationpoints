<#
.SYNOPSIS
This script will be used by nightly pipeline to compile and run UI RelativitySync Toggle On/Off tests
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("On","Off")]
    [string] $Toggle
)
function Invoke-Task ($Task) {
    $TaskRunner = Resolve-Path -Path build.ps1
    &($TaskRunner) $Task -Configuration Release
}

Invoke-Task Compile

Invoke-Task Test
