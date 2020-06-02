<#
.SYNOPSIS
This script will be used by nightly pipeline to compile and run UI Web Import/Export tests
#>

$TaskRunner = Resolve-Path -Path build.ps1

&($TaskRunner) -Configuration Release

&($TaskRunner) MyTest -Configuration Release -TestFilter "cat == WebImportExport && cat != NotWorkingOnTrident"