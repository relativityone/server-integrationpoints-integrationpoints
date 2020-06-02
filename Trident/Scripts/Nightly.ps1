<#
.SYNOPSIS
This script will be used by nightly pipeline to complie and run Integration tests
#>

function Invoke-Build {

    &($TaskRunner) -Configuration Release
}

function Invoke-Test ($TestFilter) {
    $TaskRunner = Resolve-Path -Path build.ps1
    &($TaskRunner) MyTest -Configuration Release -TestFilter $TestFilter
}

$TaskRunner = Resolve-Path -Path build.ps1

&($TaskRunner) -Configuration Release

&($TaskRunner) MyTest -Configuration Release -TestFilter "(namespace =~ FunctionalTests || namespace =~ /Tests\.Integration[\$\.]/ || namespace =~ E2ETests) && cat != NotWorkingOnTrident"