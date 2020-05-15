<#
.SYNOPSIS
This script will be used by regression pipeline to compile and run Sync UI tests on regression environment
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $RegEnv
)

Import-Module (Join-Path $PSScriptRoot Build-Util.psm1)

Set-RegressionSettings $RegEnv

Invoke-Task Compile

Invoke-Test "cat == ExportToRelativity && cat != NotWorkingOnRegressionEnvironment"

Remove-Module Build-Util