#Requires -Version 5.0

<#
.SYNOPSIS
    Builds all solutions found in source directory and signs DLLs

.PARAMETER version
    Build version number for DLL

.PARAMETER packageVersion
    Version used to describe package (for example 1.2.3-dev-4)

.PARAMETER buildConf
    Build configuration - Debug or Release

.PARAMETER sourceDir
    Path to source directory

.PARAMETER certThumbprint
    Thumbprint of certificate used for signing DLLs. In case thumbprint is empty, signing will be skipped

.PARAMETER signToolPath
    Path to signtool.exe file
#>

[CmdletBinding()]
param(
    [string]$version,
    [string]$packageVersion,
    [string]$buildConf,
    [string]$sourceDir,
    [string]$certThumbprint,
    [string]$signToolPath
)

Get-ChildItem -Path $sourceDir -Filter *.sln -File | ForEach-Object {
    & $global:msbuild_exe $_.FullName "/p:Configuration=$buildConf" "/p:AssemblyVersion=$version" "/p:InformationVersion=$packageVersion" `
        "/p:CertificateThumbprint=$certThumbprint" "/p:SignToolPath=$signToolPath" "/nologo" "/nodereuse:false" "/maxcpucount"

    if ($LASTEXITCODE -ne 0) {
        Throw "An error occured while building solution $($_.FullName)."
    }
}
