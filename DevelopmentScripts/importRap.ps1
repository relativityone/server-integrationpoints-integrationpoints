[CmdletBinding()]
param(
    [Parameter(Mandatory = $True)]
    [String] $ServerName,

    [Parameter(Mandatory = $True)]
    [String] $RAPPath,

    [Parameter(Mandatory = $True)]
    [String] $AdminUserName,

    [Parameter(Mandatory = $True)]
    [String] $AdminPwd
)

$installationTimeOut = 600
$SkipCustomPageRefresh = $False
$SkipWorkspaceUpgrade = $False

add-type @"
using System.Net;
using System.Security.Cryptography.X509Certificates;
public class TrustAllCertsPolicy : ICertificatePolicy {
    public bool CheckValidationResult(
        ServicePoint srvPoint, X509Certificate certificate,
        WebRequest request, int certificateProblem) {
        return true;
    }
}
"@
$AllProtocols = [System.Net.SecurityProtocolType]'Ssl3,Tls,Tls11,Tls12'
[System.Net.ServicePointManager]::SecurityProtocol = $AllProtocols
[System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy

$modulePath = Join-Path -Path $PSScriptRoot -ChildPath "RAPTools\*\RAPTools.psd1"
Import-Module -Name $modulePath -Force

$AdminPassword = ConvertTo-SecureString $AdminPwd -AsPlainText -Force

$relativityUserCredential = New-Object System.Management.Automation.PSCredential($AdminUserName, $AdminPassword)
try {
    $rapAppInfo = Get-RelativityRAPInfo -FilePath $RAPPath
    $libAppInfo = Get-RelativityLibraryApplication -HostName $ServerName -ApplicationName $rapAppInfo.Name -RelativityCredential $relativityUserCredential
    if(($null -ne $libAppInfo) -AND ([System.Version]$libAppInfo.version -gt [System.Version]$rapAppInfo.Version))
    {
        throw "Cannot downgrade application for $ServerName. Current installed version: $($libAppInfo.version), rap version $($rapAppInfo.Version)"
    }
    $appInfo = Install-RelativityLibraryApplication -HostName $ServerName -FilePath $RAPPath -RelativityCredential $relativityUserCredential -InstallationTimeOut $installationTimeOut  -Force -SkipCustomPageRefresh:$SkipCustomPageRefresh
    if(-not $SkipWorkspaceUpgrade)
    {
        try
        {
            Start-RelativityWorkspaceApplicationUpgrade -HostName $ServerName -ApplicationGUID $appInfo.GUID -RelativityCredential $relativityUserCredential
        }
        catch
        {
            throw "Application has been imported to the library successfully, but job failed on workspace upgrade, $_"
        }
    }
}
catch
{
    Write-Host "Failed to import RAP into relativity instance."
    throw $_
}
$appInfo