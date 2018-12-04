#Requires -Version 5.0

<#
.SYNOPSIS
    Runs SonarQube analysis

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
    [string]$version,
    [string]$coverageFileName
)

$url = "https://sonarqube.kcura.corp"
$token = "f3dc8b5d1dafdb4bdf42465d2b9eb105478d915d"

Write-Verbose "Looking for Sonar Scanner executable..."
$sonarScannerExe = (Get-ChildItem -Path $toolsDir -Filter "SonarScanner.MSBuild.exe" -File -Recurse).FullName
if (!$sonarScannerExe) {
    throw "Cannot find Sonar Scanner executable."
}

Write-Verbose "Looking for test coverage report..."
$testCoverageReport = (Get-ChildItem -Path $logsDir -Filter $coverageFileName -File -Recurse).FullName
if (!$testCoverageReport) {
    throw "Cannot find test coverage report."
}

Write-Verbose "Running Sonar Scanner for version $version..."
& $sonarScannerExe begin /k:$projectName `
    /n:$projectName `
    /v:$version `
    /d:sonar.login=$token `
    /d:sonar.host.url=$url `
    /d:sonar.language=cs `
    /d:sonar.exclusions="Source/**/obj/**/*,Source/**/bin/**/*" `
    /d:sonar.cs.dotcover.reportsPaths=$testCoverageReport

if ($LASTEXITCODE -ne 0) {
    Throw "An error occured while starting sonar scanner."
}

$sonarBuildDir = Join-Path $logsDir "sonarBuild"

Get-ChildItem -Path $sourceDir -Filter *.sln -File | ForEach-Object {
    & $global:msbuild_exe $_.FullName "/p:Configuration=Release" "/p:OutputPath=$sonarBuildDir" "/nologo" "/nodereuse:false" "/maxcpucount"
    
    if ($LASTEXITCODE -ne 0) {
        Throw "An error occured while scanning solution $($_.FullName)."
    }
}

& $sonarScannerExe end /d:sonar.login=$token

if ($LASTEXITCODE -ne 0) {
    Throw "An error occured while ending sonar scanner."
}

