#Requires -Version 5.0

<#
.SYNOPSIS
    Signs and publishes NuGet packages

.PARAMETER nugetExe
    Path to nuget.exe file

.PARAMETER nugetOutput
    Path to directory where NuGet packages were created

.PARAMETER certName
    Name of the certificate used to sign NuGet packages

.PARAMETER url
    proget server URL

.PARAMETER apiKey
    API-key used to authenticate to proget server
#>

[CmdletBinding()]
param(
    [string]$nugetExe,
    [string]$nugetOutput,
    [string]$certName,
    [string]$url,
    [string]$apiKey
)

# When symbols package is pushed, ProGet creates package without symbols automatically
# So we need to push only one package for each NuGet
Get-ChildItem -Path $nugetOutput -Filter "*.symbols.nupkg" | ForEach-Object {
    & $nugetExe sign -CertificateSubjectName $certName -Timestamper "http://timestamp.digicert.com" $_.FullName

    if ($LASTEXITCODE -ne 0) {
        Throw "An error occured while signing NuGet."
    }

    & $nugetExe push $_.FullName -Source $url -ApiKey $apiKey

    if ($LASTEXITCODE -ne 0) {
        Throw "An error occured while publishing NuGet."
    }
}
