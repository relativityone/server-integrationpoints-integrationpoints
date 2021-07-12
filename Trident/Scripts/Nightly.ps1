<#
.SYNOPSIS
This script will be used by nightly pipeline to complie and run Integration tests
#>
$TaskRunner = Resolve-Path -Path build.ps1

&($TaskRunner) Compile, Package, CIFunctionalTest -Configuration Release