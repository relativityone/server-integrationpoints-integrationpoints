#Requires -Version 5.0

<#
.SYNOPSIS
    Runs SonarQube analysis

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
    [string]$logsDir,
    [string]$version
)

$projectKey = "Relativity.Sync"
$projectName = "Relativity Sync"
$url = "https://sonarqube.kcura.corp"
$token = "f3dc8b5d1dafdb4bdf42465d2b9eb105478d915d"

Write-Verbose "Looking for Sonar Scanner executable..."
$sonarScannerExe = (Get-ChildItem -Path $toolsDir -Filter "SonarScanner.MSBuild.exe" -File -Recurse).FullName
if (!$sonarScannerExe) {
    throw "Cannot find Sonar Scanner executable."
}

Write-Verbose "Looking for test coverage report..."
$testCoverageReport = (Get-ChildItem -Path $logsDir -Filter "coverage.xml" -File -Recurse).FullName
if (!$testCoverageReport) {
    throw "Cannot find test coverage report."
}

Write-Verbose "Running Sonar Scanner for version $version..."
& $sonarScannerExe begin /k:$projectKey `
    /n:$projectName `
    /v:$version `
    /d:sonar.login=$token `
    /d:sonar.host.url=$url `
    /d:sonar.language=cs `
    /d:sonar.exclusions="Source/**/obj/**/*,Source/**/bin/**/*" `
    /d:sonar.cs.dotcover.reportsPaths=$testCoverageReport

$sonarBuildDir = Join-Path $logsDir "sonarBuild"

Get-ChildItem -Path $sourceDir -Filter *.sln -File | ForEach-Object {
    exec { msbuild @($_.FullName,
            ("/p:Configuration=Release"),
            ("/p:OutputPath=$sonarBuildDir"),
            ("/nodereuse:false"),
            ("/nologo"),
            ("/maxcpucount"))
    }
}

& $sonarScannerExe end /d:sonar.login=$token

