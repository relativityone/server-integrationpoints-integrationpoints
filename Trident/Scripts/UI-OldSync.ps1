<#
.SYNOPSIS
This script will be used by nightly pipeline to compile and run UI RelativitySync Toggle On/Off tests
#>

$TaskRunner = Resolve-Path -Path build.ps1

&($TaskRunner) Compile, Package, MyTest -Configuration Release -TestFilter "cat == RIP_OLD && cat != NotWorkingOnTrident && cat != InQuarantine"