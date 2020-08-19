<#
.SYNOPSIS
This script will be used by nightly pipeline to complie and run RelativitySync tests
#>

$TaskRunner = Resolve-Path -Path build.ps1

&($TaskRunner) PerformanceTest -Configuration Release