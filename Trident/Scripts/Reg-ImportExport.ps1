<#
.SYNOPSIS
This script will be used by regression pipeline to compile and run RIP UI tests on regression environment
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $RegEnv
)

function Invoke-Build {
    $TaskRunner = Resolve-Path -Path build.ps1
    &($TaskRunner) -Configuration Release
}

function Invoke-Test ($TestFilter) {
    $TaskRunner = Resolve-Path -Path build.ps1
    &($TaskRunner) RegTest -Configuration Release -TestFilter $TestFilter
}

Import-Module (Join-Path $PSScriptRoot Build-Util.psm1)

Set-RegressionSettings $RegEnv

Invoke-Build

Invoke-Test "cat == WebImportExport && cat != NotWorkingOnRegressionEnvironment"

Remove-Module Build-Util