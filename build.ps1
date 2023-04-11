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

. $profile
Set-StrictMode -Version 2.0

$ToolsDir = Join-Path $PSScriptRoot "buildtools"
$ReportGenerator = Join-Path $ToolsDir "reportgenerator.exe"
Import-Module -Force "$ToolsDir\BuildHelpers.psm1" -ErrorAction Stop
Install-NugetPackage -Name kCura.PSBuildTools -Version 0.9.8 -ToolsDir $ToolsDir -ErrorAction Stop
Import-Module (Join-Path $ToolsDir "kCura.PSBuildTools.*\PSBuildTools.psd1") -ErrorAction Stop
Install-NugetPackage -Name psake-rel -Version 5.0.0 -ToolsDir $ToolsDir -ErrorAction Stop
Import-Module (Join-Path $ToolsDir "psake-rel.*\tools\psake\psake.psd1") -ErrorAction Stop
if (-not (Test-Path $ReportGenerator))
{
    & dotnet tool install dotnet-reportgenerator-globaltool --version 4.1.9 --tool-path $ToolsDir
    if ($LASTEXITCODE -ne 0) { throw "An error occured while restoring build tools." }
}

$Params = @{
    taskList = $TaskList
    nologo = $true
    parameters = @{	
        BuildConfig = $Configuration
        ReportGenerator = $ReportGenerator
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
