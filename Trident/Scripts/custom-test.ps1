[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $TestFilter
)

Write-Host $TestFilter