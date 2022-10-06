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

&($TaskRunner) Compile, Package, RegTest -Configuration Release -TestFilter "cat == Regression"

Remove-Module Build-Util