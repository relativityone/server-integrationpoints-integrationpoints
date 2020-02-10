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

Import-Module (Join-Path $PSScriptRoot Build-Util.psm1)

Invoke-Task Compile

Invoke-Task Test

Invoke-Task Package

if($Toggle -eq "On") {
    Set-TestSetting -Name SyncEnabled -Value true
}
elseif($Toggle -eq "Off") {
    Set-TestSetting -Name SyncEnabled -Value false
}

Invoke-Task UIRelativitySyncTest

Remove-Module Build-Util