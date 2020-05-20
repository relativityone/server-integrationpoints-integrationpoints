<#
.SYNOPSIS
This script will be used by nightly pipeline to complie and run Integration tests
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

Invoke-Task OneTimeTestsSetup

Invoke-Test "namespace =~ FunctionalTests && namespace =~ /Tests\.Integration[\$\.]/ && namespace =~ E2ETests && cat != NotWorkingOnTrident"