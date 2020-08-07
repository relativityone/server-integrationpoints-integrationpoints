<#
.SYNOPSIS
This script will be used by regression pipeline to compile and run RIP UI tests on regression environment
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $RegEnv
)

Import-Module (Join-Path $PSScriptRoot Build-Util.psm1)

Set-RegressionSettings $RegEnv

$TaskRunner = Resolve-Path -Path build.ps1

&($TaskRunner) -Configuration Release

#&($TaskRunner) RegTest -Configuration Release -TestFilter "cat == Test" #"cat == WebImportExport && cat != NotWorkingOnRegressionEnvironment"

Remove-Module Build-Util