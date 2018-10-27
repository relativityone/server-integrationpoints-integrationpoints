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
    Build type - alpha, beta, rc or release

.PARAMETER version
    Allows to manually set build version number
#>

[CmdletBinding()]
param(
    [string[]]$taskList = @(),

    [ValidateSet("Debug","Release")]
    [string]$buildConfig = "Debug",
    
    [ValidateSet("alpha", "beta", "rc", "release")]
    [string]$buildType = "alpha",

    [string]$version = "0.0.0.0"
)

$BASE_DIR = Resolve-Path .
$TOOLS_DIR = Join-Path $BASE_DIR "buildtools"
$SCRIPTS_DIR = Join-Path $BASE_DIR "scripts"
$SOURCE_DIR = Join-Path $BASE_DIR "Source"

Write-Verbose "Restoring buildtools..."
& "$SCRIPTS_DIR\restore-buildtools.ps1" -toolsDir $TOOLS_DIR

Write-Verbose "Importing powershell modules..."
& "$SCRIPTS_DIR\import-build-modules.ps1" -toolsDir $TOOLS_DIR

Write-Verbose "Executing build..."
Invoke-PSake "default.ps1" `
	-parameters @{	'root' = $BASE_DIR;
                    'toolsDir' = $TOOLS_DIR;
                    'scriptsDir' = $SCRIPTS_DIR;
                    'sourceDir' = $SOURCE_DIR; 
                    'buildConfig' = $buildConfig;
                    'buildType' = $buildType;
                    'version' = $version }`
	-taskList $taskList `

exit ( [int]( -not $psake.build_success ) )