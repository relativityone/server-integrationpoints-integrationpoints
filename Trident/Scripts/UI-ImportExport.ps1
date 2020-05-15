<#
.SYNOPSIS
This script will be used by nightly pipeline to compile and run UI Web Import/Export tests
#>

Import-Module (Join-Path $PSScriptRoot Build-Util.psm1)

Invoke-Task Compile

Invoke-Task Test

Invoke-Task Package

Invoke-Task OneTimeTestsSetup

Invoke-Test "cat == WebImportExport"

Remove-Module Build-Util