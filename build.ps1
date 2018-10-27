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
$NUGET_EXE = Join-Path $TOOLS_DIR "nuget.exe"

Write-Verbose "Restoring buildtools..."
& "$SCRIPTS_DIR\restore-buildtools.ps1" -nugetExe $NUGET_EXE -toolsDir $TOOLS_DIR

Write-Verbose "Importing powershell modules..."
& "$SCRIPTS_DIR\import-build-modules.ps1" -toolsDir $TOOLS_DIR

Write-Verbose "Executing build..."
Invoke-PSake "default.ps1" `
	-parameters @{	'root' = $BASE_DIR;
					'toolsDir' = $TOOLS_DIR;
					'nugetExe' = $NUGET_EXE }`
	-properties @{	'buildConfig' = $buildConfig;
	                'buildType' = $buildType;
					'version' = $version }`
	-nologo `
	-taskList $taskList `

exit ( [int]( -not $psake.build_success ) )