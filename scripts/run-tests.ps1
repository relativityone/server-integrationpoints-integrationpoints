#Requires -Version 5.0

<#
.SYNOPSIS
    Runs tests

.PARAMETER projectName
    Project name

.PARAMETER testsType
    Tests type - integration or performance

.PARAMETER sourceDir
    Path to source directory

.PARAMETER toolsDir
    buildtools directory

.PARAMETER logsDir
    logs directory

.PARAMETER sutAddress
    Hostname of the target Relativity instance
#>

[CmdletBinding()]
param(
    [string]$projectName,
    [ValidateSet("Integration", "Performance", "System")]
    [string]$testsType,

    [string]$sourceDir,
    [string]$toolsDir,
    [string]$logsDir,
    [string]$sutAddress
)

Write-Verbose "Looking for $testsType tests DLLs..."
$testsDlls = (Get-ChildItem -Path $sourceDir -Filter "$projectName*Tests.$testsType.dll" -File -Recurse | Where-Object { $_.Directory.Parent -match "bin" }).FullName
if (!$testsDlls) {
    Throw "Unable to locate any $testsType tests DLLs."
}
else {
    Write-Verbose "Found $testsType tests DLLs: $testsDlls"
}

if ($sutAddress) {
    Write-Verbose "Looking for .config files to edit..."
    foreach ($dll in $testsDlls) {
        $configPath = "$dll.config"
        if (Test-Path $configPath) { 
            $configXml = [xml]$(Get-Content $configPath)
            $hostNode = $configXml.SelectSingleNode("//add[@key='RelativityHostName']")
            if($hostNode){
                $hostNode.value = $sutAddress
                $configXml.Save($configPath)
            }
        }
        else {
            Write-Verbose ".config file for '$dll' dll not found."
        }
    }
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
