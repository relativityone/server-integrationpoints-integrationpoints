#Requires -Version 5.0

<#
.SYNOPSIS
    Looks for certificate to sign DLLs and return it's thumbprint
#>

$certificateName = '*Relativity ODA LLC*'

Write-Verbose "Looking for certificate named $certificateName..."
$certificateObject = Get-ChildItem -Path cert: -Recurse | Where-Object {$_.FriendlyName -like $certificateName} | Select-Object -first 1
if ($certificateObject) {
    Write-Verbose "Certificate found"
    return $certificateObject.Thumbprint
}
else {
    Throw "Certificate $certificateName not found. Unable to sign DLLs."
}