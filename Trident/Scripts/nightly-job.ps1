<#
.SYNOPSIS
This script will be used by nightly pipeline to complie and run RelativitySync tests
#>

function Invoke-Task ($Task) {
    $TaskRunner = Join-Path $env:workspace build.ps1
    &($TaskRunner) $Task -Configuration Release
}

Invoke-Task Compile

Invoke-Task Test

Invoke-Task FunctionalTest