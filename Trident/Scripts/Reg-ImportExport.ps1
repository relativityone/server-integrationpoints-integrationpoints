<#
.SYNOPSIS
This script will be used by regression pipeline to compile and run RIP UI tests on regression environment
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $RegEnv
)

function Invoke-Task ($Task) {
    $TaskRunner = Resolve-Path -Path build.ps1
    &($TaskRunner) $Task -Configuration Release
}

function Invoke-Test ($TestFilter) {
    $TaskRunner = Resolve-Path -Path build.ps1
    &($TaskRunner) CustomTest -Configuration Release -TestFilter $TestFilter
}

$path = Test-Path "\\bld-pkgs\Packages\IntegrationPoints"
$path
# Import-Module (Join-Path $PSScriptRoot Build-Util.psm1)

# Set-RegressionSettings $RegEnv

# Invoke-Task Compile

# Invoke-Test "cat == WebImportExport && cat != NotWorkingOnRegressionEnvironment && cat == ImportFromLoadFile"

# Remove-Module Build-Util