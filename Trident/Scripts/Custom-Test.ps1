<#
.SYNOPSIS
This script will be used by nightly pipeline to complie and run Integration tests
#>

$TaskRunner = Resolve-Path -Path build.ps1

&($TaskRunner) -Configuration Release

&($TaskRunner) MyTest -Configuration Release -TestFilter "(namespace =~ FunctionalTests || namespace =~ /Tests\.Integration[\$\.]/ || namespace =~ E2ETests) && cat != NotWorkingOnTrident" #"cat == Test"