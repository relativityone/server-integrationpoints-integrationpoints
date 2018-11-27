#Requires -Version 5.0

<#
.SYNOPSIS
    Looks for certificate to sign DLLs and return it's thumbprint

.PARAMETER certName
    Name of certificate for signing
#>

[CmdletBinding()]
param(
    [string]$certName
)

Write-Verbose "Looking for certificate named $certName..."
$certificateObject = Get-ChildItem -Path cert: -Recurse | Where-Object {$_.SubjectName -like "*$certName*"} | Select-Object -first 1
if ($certificateObject) {
    Write-Verbose "Certificate found"
    $global:certThumbprint = $certificateObject.Thumbprint
    return
}
else {
    Throw "Certificate $certName not found. Unable to sign DLLs."
}