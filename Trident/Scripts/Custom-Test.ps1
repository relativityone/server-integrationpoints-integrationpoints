<#
.SYNOPSIS
This script will be used by nightly pipeline to complie and run Integration tests
#>

$TaskRunner = Resolve-Path -Path build.ps1

&($TaskRunner) -Configuration Release

&($TaskRunner) MyTest -Configuration Release -TestFilter "cat == Test"