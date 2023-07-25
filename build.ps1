##########################################################################
# This is the Psake bootstrapper script for PowerShell.
##########################################################################

<#
.SYNOPSIS
This is a Powershell script to bootstrap a Psake build.

.DESCRIPTION
This Powershell script will download NuGet if missing, restore build tools using Nuget
and execute your build tasks with the parameters you provide.

.PARAMETER TaskList
List of build tasks to execute.

.PARAMETER Configuration
The build configuration to use. Either Debug or Release. Defaults to Debug.

.LINK

#>

[CmdletBinding()]
param(
    [string[]]$taskList = @(),

    [Parameter(Mandatory=$False)]
    [ValidateSet("Debug","Release")]
    [string]$Configuration = "Debug",

    # <-- Test section -->
	[Parameter(Mandatory=$False)]
	[String]$TestFilter
)

Set-StrictMode -Version 2.0

$BaseDir = $PSScriptRoot
$NugetUrl = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
$ToolsDir = Join-Path $BaseDir "buildtools"
$NugetExe = Join-Path $ToolsDir "nuget.exe"

$ToolsConfig = Join-Path $ToolsDir "packages.config"

Write-Progress "Checking for NuGet in tools path..."
if (-Not (Test-Path $NugetExe -Verbose:$VerbosePreference)) {
	Write-Progress "Installing NuGet from $NugetUrl..."
	Invoke-WebRequest $NugetUrl -OutFile $NugetExe -Verbose:$VerbosePreference -ErrorAction Stop
}

Write-Progress "Restoring tools from NuGet..."
$NuGetVerbosity = if ($VerbosePreference -gt "SilentlyContinue") { "normal" } else { "quiet" }
& $NugetExe install $ToolsConfig -o $ToolsDir -ExcludeVersion -Verbosity $NuGetVerbosity

if ($LASTEXITCODE -ne 0) {
	Throw "An error occured while restoring NuGet tools."
}

Write-Progress "Importing required Powershell modules..."
$ToolsDir = Join-Path $PSScriptRoot "buildtools"
Import-Module (Join-Path $ToolsDir "psake-rel\tools\psake\psake.psd1") -ErrorAction Stop
Import-Module (Join-Path $ToolsDir "kCura.PSBuildTools\PSBuildTools.psd1") -ErrorAction Stop
Install-Module VSSetup -Scope CurrentUser -Force

$Params = @{
    taskList = $TaskList
    nologo = $true
    parameters = @{	
        BuildConfig = $Configuration
        BuildToolsDir = $ToolsDir
		# <-- Test section -->
		TestFilter = $TestFilter
    }
    Verbose = $VerbosePreference
    Debug = $DebugPreference
}

Try
{
    Invoke-PSake @Params
}
Finally
{
    $ExitCode = 0
    If ($psake.build_success -eq $False)
    {
        $ExitCode = 1
    }

    Remove-Module PSake -Force -ErrorAction SilentlyContinue
    Remove-Module PSBuildTools -Force -ErrorAction SilentlyContinue
}

Exit $ExitCode
