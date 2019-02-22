#Requires -Version 5.0

<#
.SYNOPSIS
    Analyzis the given solution to figure out whether or not it is missing ConfigureAwait

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
    [string]$logsDir
)

$inspectCodeFileName = "inspectcode.exe"
$configureAwaitPluginDir = "ConfigureAwaitChecker*"

Write-Verbose "Looking for $inspectCodeFileName file..."
$inspectCode = Get-ChildItem -Path $toolsDir -Filter $inspectCodeFileName -File -Recurse
if (!$inspectCode) {
    throw "Cannot find $inspectCodeFileName file"
}

Write-Verbose "Looking for ConfigureAwaitChecker file..."
$pluginPath = (Join-Path $toolsDir $configureAwaitPluginDir)
$pluginPath = "$pluginPath\*.nupkg"
$configureAwaitPlugin = Get-ChildItem -Path $pluginPath
if (!$configureAwaitPlugin) {
    throw "Cannot find $configureAwaitPluginFileName file"
}

Write-Verbose "Copying $configureAwaitPluginFileName to inspect code tools.."
Copy-Item $configureAwaitPlugin.FullName -Destination $inspectCode.DirectoryName

Write-Verbose "Running code inspection for all solutions..."
$errorFile = Join-Path $logsDir "await.xml"

$cacheDir = Join-Path $logsDir "cache"
if (!(Test-Path -Path $cacheDir)) {
    New-Item -ItemType directory -Path $cacheDir
}
try {
    Get-ChildItem -Path $sourceDir -Filter *.sln -File | ForEach-Object {
        Write-Verbose "Restoring packages..."
        & dotnet restore $_.FullName

        Write-Verbose "Running code inspection for $_..."
        & $inspectCode.FullName $_.FullName --output=$errorFile --s="SUGGESTION" --caches-home="$cacheDir"

        if ($LASTEXITCODE -ne 0) {
            Throw "An error occured while analyzing solution $($_.FullName)."
        }

        [xml]$xmlErrors = Get-Content -Path $errorFile

        if ($xmlErrors.Report.IssueTypes | Select-Xml -XPath "//IssueType[@Id='ConsiderUsingConfigureAwait']" -ErrorAction Ignore) {
            throw "Build failed. ConfigureAwait missing. Details about where ConfigureAwait needs to be added can be found at $errorFile"
        }
    }
} finally {
    Remove-Item -Recurse -Force $cacheDir
}