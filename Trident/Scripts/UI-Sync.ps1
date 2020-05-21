<#
.SYNOPSIS
This script will be used by nightly pipeline to compile and run UI RelativitySync Toggle On/Off tests
#>

function Invoke-Task ($Task) {
    $TaskRunner = Resolve-Path -Path build.ps1
    &($TaskRunner) $Task -Configuration Release
}

function Invoke-Test ($TestFilter) {
    $TaskRunner = Resolve-Path -Path build.ps1
    &($TaskRunner) CustomTest -Configuration Release -TestFilter $TestFilter
}

Invoke-Task Compile

Invoke-Task Test

Invoke-Task Package

#Invoke-Task OneTimeTestsSetup

Invoke-Test "cat == ExportToRelativity && cat != NotWorkingOnTrident"