<#
.SYNOPSIS
This script will be used by nightly pipeline to complie and run Integration tests
#>

function Invoke-Task ($Task) {
    $TaskRunner = Resolve-Path -Path build.ps1
    &($TaskRunner) $Task -Configuration Release
}

Invoke-Task Compile

Invoke-Task Test

Invoke-Task Package

Invoke-Task FunctionalTest