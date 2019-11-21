<#
.SYNOPSIS
This script will be used by nightly pipeline to complie and run RelativitySync tests
#>

$TaskRunner = Join-Path $env:workspace build.ps1;

Write-Host "------- Executing: Compile RelativitySync -------"
&($TaskRunner) Compile -Configuration Release 

Write-Host "------- Executing: Run Unit Tests -------"
&($TaskRunner) UnitTest

Write-Host "------- Executing: Run Integration Tests -------"
&($TaskRunner) IntegrationTest