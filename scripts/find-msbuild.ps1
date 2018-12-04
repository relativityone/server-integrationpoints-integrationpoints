#Requires -Version 5.0

<#
.SYNOPSIS
    Looks for newest MSBuild file
#>

[CmdletBinding()]
param()

$agentPath = "$Env:programfiles (x86)\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin\msbuild.exe"
$devPath = "$Env:programfiles (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\msbuild.exe"
$proPath = "$Env:programfiles (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\msbuild.exe"
$communityPath = "$Env:programfiles (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\msbuild.exe"
$fallback2015Path = "${Env:ProgramFiles(x86)}\MSBuild\14.0\Bin\MSBuild.exe"
$fallback2013Path = "${Env:ProgramFiles(x86)}\MSBuild\12.0\Bin\MSBuild.exe"
$fallbackPath = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"

If (Test-Path $agentPath) {
    Write-Verbose "Found MSBuild 15.0"
    $global:msbuild_exe = $agentPath
    return
}
If (Test-Path $devPath) {
    Write-Verbose "Found MSBuild 15.0"
    $global:msbuild_exe = $devPath
    return
}
If (Test-Path $proPath) {
    Write-Verbose "Found MSBuild 15.0"
    $global:msbuild_exe = $proPath
    return
}
If (Test-Path $communityPath) {
    Write-Verbose "Found MSBuild 15.0"
    $global:msbuild_exe = $communityPath
    return
}
If (Test-Path $fallback2015Path) {
    Write-Verbose "Found MSBuild 14.0"
    $global:msbuild_exe = $fallback2015Path
    return
}
If (Test-Path $fallback2013Path) {
    Write-Verbose "Found MSBuild 12.0"
    $global:msbuild_exe = $fallback2013Path
    return
}
If (Test-Path $fallbackPath) {
    Write-Verbose "Found MSBuild 4.0"
    $global:msbuild_exe = $fallbackPath
    return
}

throw "Unable to find msbuild"