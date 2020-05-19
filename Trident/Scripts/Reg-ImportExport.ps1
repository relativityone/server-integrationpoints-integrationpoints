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

$transferConsole = ".\Source\kCura.IntegrationPoint.Tests.Core\ExternalDependencies\TransferConsole\Relativity.Transfer.Console.exe"

$sourcePath = ".\Source\kCura.IntegrationPoint.Tests.Core\TestDataImportFromLoadFile"
$workspaceId = 3026063
$url = "https://regression-a.r1.kcura.com"
$userName = "rip.jenkins@rip.com"
$password = "Test1234!"

& $transferConsole "/interactive:-" "/command:transfer" "/url:""$url""" "/username:""$userName""" "/password:""$password""" "/direction:Upload" "/configuration:""client=Aspera""" "/searchpath:""$sourcePath""" "/targetpath:""Files\EDDS$workspaceId\DataTransfer\Import""" "/workspaceid:$workspaceId"















# Import-Module (Join-Path $PSScriptRoot Build-Util.psm1)

# Set-RegressionSettings $RegEnv

# Invoke-Task Compile

# Invoke-Test "cat == WebImportExport && cat != NotWorkingOnRegressionEnvironment && cat == ImportFromLoadFile"

# Remove-Module Build-Util