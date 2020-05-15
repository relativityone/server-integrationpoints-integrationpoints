<#
.SYNOPSIS
This script will be used by nightly pipeline to complie and run Integration tests
#>

Import-Module (Join-Path $PSScriptRoot Build-Util.psm1)

Invoke-Task Compile

Invoke-Task Test

Invoke-Task Package

Invoke-Test "namespace =~ FunctionalTests && namespace =~ /Tests\.Integration[\$\.]/ && namespace =~ E2ETests"

Remove-Module Build-Util