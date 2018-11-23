#Requires -Version 5.0

<#
.SYNOPSIS
    Looks for singtool.exe file
#>

[CmdletBinding()]
param()

$clickOnceDir = "C:\Program Files (x86)\Microsoft SDKs\ClickOnce\SignTool\signtool.exe"
Write-Verbose "Looking for signtool.exe in $clickOnceDir..."
if (Test-Path -Path $clickOnceDir) {
    $global:signToolPath = $clickOnceDir
    return
}

$sdkDirs = "C:\Program Files*\Windows Kits\*\bin\*\signtool.exe"
Write-Verbose "Looking for signtool.exe in $sdkDirs..."
$sdkPaths = (Resolve-Path -Path $sdkDirs).Path
if ($sdkPaths) {
    $global:signToolPath = $sdkPaths[0]
    return
}

throw "Cannot find signtool.exe file. Use verbose mode to show paths checked."