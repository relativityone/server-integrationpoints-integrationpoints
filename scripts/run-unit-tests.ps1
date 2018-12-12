#Requires -Version 5.0

<#
.SYNOPSIS
    Runs unit tests and generates coverage report

.PARAMETER projectName
    Project name

.PARAMETER sourceDir
    Path to source directory

.PARAMETER toolsDir
    buildtools directory

.PARAMETER logsDir
    logs directory

.PARAMETER coverageFileName
    Coverage file name
#>

[CmdletBinding()]
param(
    [string]$projectName,
    [string]$sourceDir,
    [string]$toolsDir,
    [string]$logsDir,
    [string]$coverageFileName
)

Write-Verbose "Looking for unit tests DLLs..."
$unitTestsDlls = (Get-ChildItem -Path $sourceDir -Filter "$projectName*Tests.Unit.dll" -File -Recurse | Where-Object { $_.Directory.Parent -match "bin" }).FullName
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

$coverageReportPath = Join-Path $logsDir $coverageFileName

Write-Verbose "Running unit tests..."
& $dotCover cover /TargetExecutable=$nunitConsoleRunner `
    /Output=$coverageReportPath `
    /ReportType="HTML" `
    /TargetArguments="$unitTestsDlls --skipnontestassemblies --work=$logsDir" `
    /Filters="+:$projectName*;-:$projectName*.Tests*" `
    /AttributeFilters=System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute

if ($LASTEXITCODE -ne 0) {
    Throw "An error occured during unit tests execution."
}