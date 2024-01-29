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

.PARAMETER PackageVersion
Version to use in the nuget package during the Package task.

.PARAMETER RAPVersion
Version to use for the RAP during the Package task.

.PARAMETER Configuration
The build configuration to use. Either Debug or Release. Defaults to Debug.

.LINK

#>

[CmdletBinding()]
param(
	[string[]]$taskList = @(),
	
	
	[Parameter(Mandatory=$False)]
	[String]$RAPVersion = "1.0.0.0",

	[Parameter(Mandatory=$False)]
	[String]$PackageVersion = "1.0.0",
	
	[Parameter(Mandatory=$False)]
	[ValidateSet("Debug","Release")]
	[string]$Configuration = "Debug",
	
	# <-- Test section -->
	[Parameter(Mandatory=$False)]
	[String]$TestFilter
	)

. $profile
Set-StrictMode -Version 2.0

$BaseDir = $PSScriptRoot
$ToolsDir = Join-Path $BaseDir 'buildtools'
$NuGetFolder = Join-path $ToolsDir 'NuGet'
$NugetExe = Join-Path $NuGetFolder 'nuget.exe'
$NugetUrl = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"

$ToolsConfig = Join-Path $ToolsDir "packages.config"

Write-Host "Checking for NuGet in tools path..."
if (-Not (Test-Path $NugetExe -Verbose:$VerbosePreference)) {
	Write-Host "Installing NuGet from $NugetUrl..."
	Invoke-WebRequest $NugetUrl -OutFile $NugetExe -Verbose:$VerbosePreference -ErrorAction Stop
}

Write-Host "Restoring tools from NuGet..."
$NuGetVerbosity = if ($VerbosePreference -gt "SilentlyContinue") { "normal" } else { "quiet" }
& $NugetExe install $ToolsConfig -o $ToolsDir -ExcludeVersion -Verbosity $NuGetVerbosity

if ($LASTEXITCODE -ne 0) {
	Throw "An error occured while restoring NuGet tools."
}

Write-Host "Importing required Powershell modules..."
$ToolsDir = Join-Path $PSScriptRoot "buildtools"
$ReportGenerator = Join-Path $ToolsDir "reportgenerator.exe"
Import-Module (Join-Path $ToolsDir "psake-rel\tools\psake\psake.psd1") -ErrorAction Stop
Install-NugetPackage -Name kCura.PSBuildTools -Version 0.9.8 -ToolsDir $ToolsDir -ErrorAction Stop
Import-Module (Join-Path $ToolsDir "kCura.PSBuildTools\PSBuildTools.psd1") -ErrorAction Stop
Install-NugetPackage -Name psake-rel -Version 5.0.0 -ToolsDir $ToolsDir -ErrorAction Stop
Import-Module -Force "$ToolsDir\NpmBuildHelpers.psm1" -ErrorAction Stop

if (!(Get-Module -Name VSSetup -ListAvailable))
{
    Install-Module VSSetup -Scope CurrentUser -Repository 'powershell-anthology' -Force
}
Import-Module VSSetup -Force

$Params = @{
	taskList = $TaskList
	nologo = $true
	framework = "4.6"
	parameters = @{	
		NugetExe = $NugetExe
		BuildConfig = $Configuration
		ReportGenerator = $ReportGenerator
		BuildToolsDir = $ToolsDir
		RAPVersion = $RAPVersion
		PackageVersion = $PackageVersion
		# <-- Test section -->
		TestFilter = $TestFilter
	}
	properties = @{
		build_config = $Configuration
	}
	Verbose = $VerbosePreference
	Debug = $DebugPreference
	buildFile = (Join-Path $BaseDir "psakefile.ps1")
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
	Remove-Module NpmBuildHelpers -Force -ErrorAction SilentlyContinue
}

Exit $ExitCode
