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
    [string]$Configuration = "Debug"
)

Set-StrictMode -Version 2.0

$ToolsDir = Join-Path $PSScriptRoot "buildtools"
$ReportGenerator = Join-Path $ToolsDir "reportgenerator.exe"
$Nuget = Join-Path $ToolsDir "nuget.exe"
$ToolsConfig = Join-Path $ToolsDir "packages.config"

$NugetUrl = "https://relativity.jfrog.io/relativity/nuget-download/v5.3.0/nuget.exe"

if (-not (Test-Path $Nuget -Verbose:$VerbosePreference))
{
    Invoke-WebRequest $NugetUrl -OutFile $Nuget -Verbose:$VerbosePreference -ErrorAction Stop
}

& $Nuget install $ToolsConfig -OutputDirectory $ToolsDir

$NUnit = Resolve-Path (Join-Path $ToolsDir "NUnit.ConsoleRunner.*\tools\nunit3-console.exe")
$OpenCover = Resolve-Path (Join-Path $ToolsDir "opencover.*\tools\OpenCover.Console.exe")

Import-Module -Force "$ToolsDir\BuildHelpers.psm1" -ErrorAction Stop
Assert-Module -Name PSBuildTools -Version 0.7.0 -Path $ToolsDir
Assert-Module -Name psake -Version 4.7.4 -Path $ToolsDir
if (-not (Test-Path $ReportGenerator))
{
    & dotnet tool install dotnet-reportgenerator-globaltool --version 4.1.5 --tool-path $ToolsDir
    if ($LASTEXITCODE -ne 0) { throw "An error occured while restoring build tools." }
}

$Params = @{
    taskList = $TaskList
    nologo = $true
    parameters = @{	
        BuildConfig = $Configuration
        ReportGenerator = $ReportGenerator
        NUnit = $NUnit
        OpenCover = $OpenCover
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
