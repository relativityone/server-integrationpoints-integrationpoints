<#
.SYNOPSIS
This script will be used by nightly pipeline to compile and run UI RelativitySync Toggle On/Off tests
#>

Import-Module (Join-Path $PSScriptRoot Build-Util.psm1)

Invoke-Task Compile

Invoke-Task Test

Invoke-Task Package

Invoke-Task OneTimeTestsSetup

Invoke-Test "cat == ExportToRelativity"

Remove-Module Build-Util