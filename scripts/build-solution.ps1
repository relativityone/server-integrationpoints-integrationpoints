#Requires -Version 5.0

<#
.SYNOPSIS
    Builds all solutions found in source directory and signs DLLs

.PARAMETER version
    Build version number for DLL

.PARAMETER infoVersion
    Version used to describe package (for example 1.2.3-alpha001)

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
    [string]$infoVersion,
    [string]$buildConf,
    [string]$sourceDir,
    [string]$certThumbprint,
    [string]$signToolPath
)

Get-ChildItem -Path $sourceDir -Filter *.sln -File | ForEach-Object {
    & msbuild $_.FullName "/p:Configuration=$buildConf" "/p:AssemblyVersion=$version" "/p:InformationVersion=$infoVersion" `
        "/p:CertificateThumbprint=$certThumbprint" "/p:SignToolPath=$signToolPath" "/nologo" "/nodereuse:false" "/maxcpucount"

    if ($LASTEXITCODE -ne 0) {
        Throw "An error occured while building solution $($_.FullName)."
    }
}
