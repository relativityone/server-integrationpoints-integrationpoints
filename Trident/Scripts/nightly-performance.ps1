<#
.SYNOPSIS
This script will be used by nightly pipeline to complie and run RelativitySync tests
#>

#$TaskRunner = Resolve-Path -Path build.ps1

Run-Command -command "Write-Host 1"

#&($TaskRunner) PerformanceTest -Configuration Release