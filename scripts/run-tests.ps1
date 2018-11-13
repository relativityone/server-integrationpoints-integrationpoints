#Requires -Version 5.0

<#
.SYNOPSIS
    Runs tests

.PARAMETER testsType
    Tests type - integration or performance

.PARAMETER sourceDir
    Path to source directory

.PARAMETER toolsDir
    buildtools directory

.PARAMETER logsDir
    logs directory
#>

[CmdletBinding()]
param(
    [ValidateSet("Integration", "Performance")]
    [string]$testsType,

    [string]$sourceDir,
    [string]$toolsDir,
    [string]$logsDir
)

Write-Verbose "Looking for $testsType tests DLLs..."
$testsDlls = (Get-ChildItem -Path $sourceDir -Filter "Relativity.Sync*Tests.$testsType.dll" -File -Recurse | Where-Object { $_.Directory.Name -match "bin" }).FullName
if (!$testsDlls) {
    Throw "Unable to locate any $testsType tests DLLs."
}
else {
    Write-Verbose "Found $testsType tests DLLs: $testsDlls"
}

Write-Verbose "Looking for NUnit console runner..."
$nunitConsoleRunner = (Get-ChildItem -Path $toolsDir -Filter nunit3-console.exe -File -Recurse).FullName
if (!$nunitConsoleRunner) {
    Throw "Unable to locate NUnit console runner."
}

Write-Verbose "Running testsType tests..."
& $nunitConsoleRunner $testsDlls --work=$logsDir --skipnontestassemblies

if ($LASTEXITCODE -ne 0) {
    Throw "An error occured during testsType tests execution."
}