<#
.SYNOPSIS
This script will be used by nightly pipeline to compile and run UI Web Import/Export tests
#>

function Invoke-Task ($Task) {
    $TaskRunner = Resolve-Path -Path build.ps1
    &($TaskRunner) $Task -Configuration Release
}

Invoke-Task Compile

Invoke-Task Test