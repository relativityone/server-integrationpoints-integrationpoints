#Requires -Version 5.0

<#
.SYNOPSIS
    Runs specified tasks.

.DESCRIPTION
    Downloads nuget.exe in case it's missing.
    Restore all buildtools dependencies.
    Imports required powershell modules.
    Runs specified tasks.

.PARAMETER taskList
    List of tasks to run

.PARAMETER buildConfig
    Build configuration - Debug or Release

.PARAMETER buildType
    Build type - DEV or GOLD

.PARAMETER branchName
    Current branch name

.PARAMETER version
    Assemblies version (1.2.3.4)

.PARAMETER packageVersion
    Package version (1.2.3.-dev-4)

.PARAMETER progetApiKey
    API-key used to authenticate to proget server
#>

[CmdletBinding()]
param(
    [string[]]$taskList = @(),

    [ValidateSet("Debug","Release")]
    [string]$buildConfig = "Debug",
    
    [ValidateSet("DEV", "GOLD")]
    [string]$buildType = "DEV",

    [string]$branchName,
    [string]$version = "0.0.0.0",
    [string]$packageVersion = "0.0-dev-0",
    [string]$progetApiKey
)

$BASE_DIR = Resolve-Path .
$TOOLS_DIR = Join-Path $BASE_DIR "buildtools"
$SCRIPTS_DIR = Join-Path $BASE_DIR "scripts"
$NUGET_EXE = Join-Path $TOOLS_DIR "nuget.exe"

Write-Verbose "Restoring buildtools..."
& (Join-Path $SCRIPTS_DIR "restore-buildtools.ps1") -toolsDir $TOOLS_DIR -nugetExe $NUGET_EXE

Write-Verbose "Importing powershell modules..."
& (Join-Path $SCRIPTS_DIR "import-build-modules.ps1") -toolsDir $TOOLS_DIR

Write-Verbose "Executing build..."
Invoke-PSake "default.ps1" `
	-parameters @{	'root' = $BASE_DIR;
                    'toolsDir' = $TOOLS_DIR;
                    'scriptsDir' = $SCRIPTS_DIR;
                    'buildConfig' = $buildConfig;
                    'buildType' = $buildType;
                    'branchName' = $branchName;
                    'version' = $version;
                    'packageVersion' = $packageVersion;
                    'nugetExe' = $NUGET_EXE;
                    'progetApiKey' = $progetApiKey }`
        -nologo `
	-taskList $taskList `

exit ( [int]( -not $psake.build_success ) )