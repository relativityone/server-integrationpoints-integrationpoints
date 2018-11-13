#Requires -Version 5.0

<#
.SYNOPSIS
    Runs unit tests and generates coverage report

.PARAMETER sourceDir
    Path to source directory

.PARAMETER toolsDir
    buildtools directory

.PARAMETER logsDir
    logs directory
#>

[CmdletBinding()]
param(
    [string]$sourceDir,
    [string]$toolsDir,
    [string]$logsDir
)

Write-Verbose "Looking for unit tests DLLs..."
$unitTestsDlls = (Get-ChildItem -Path $sourceDir -Filter Relativity.Sync*Tests.Unit.dll -File -Recurse | Where-Object { $_.Directory.Name -match "bin" }).FullName
if (!$unitTestsDlls) {
    Throw "Unable to locate any unit tests DLLs."
}
else {
    Write-Verbose "Found unit tests DLLs: $unitTestsDlls"
}

Write-Verbose "Looking for dotCover executable..."
$dotCover = (Get-ChildItem -Path $toolsDir -Filter dotCover.exe -File -Recurse).FullName
if (!$dotCover) {
    Throw "Unable to locate dotCover executable."
}

Write-Verbose "Looking for NUnit console runner..."
$nunitConsoleRunner = (Get-ChildItem -Path $toolsDir -Filter nunit3-console.exe -File -Recurse).FullName
if (!$nunitConsoleRunner) {
    Throw "Unable to locate NUnit console runner."
}

Write-Verbose "Running unit tests..."
& $dotCover cover /TargetExecutable=$nunitConsoleRunner `
    /Output=".\buildlogs\coverage.xml" `
    /ReportType="XML" `
    /TargetArguments="$unitTestsDlls" `
    /Filters="+:Relativity.Sync*;-:Relativity.Sync*.Tests*"

if ($LASTEXITCODE -ne 0) {
    Throw "An error occured during unit tests execution."
}