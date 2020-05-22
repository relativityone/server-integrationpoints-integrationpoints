<#
.SYNOPSIS
This script will be used by nightly pipeline to compile and run UI RelativitySync Toggle On/Off tests
#>

function Invoke-Build {
    $TaskRunner = Resolve-Path -Path build.ps1
    &($TaskRunner) -Configuration Release
}

function Invoke-Test ($TestFilter) {
    $TaskRunner = Resolve-Path -Path build.ps1
    &($TaskRunner) MyTest -Configuration Release -TestFilter $TestFilter
}

Invoke-Build

Invoke-Test "cat == ExportToRelativity && cat != NotWorkingOnTrident"