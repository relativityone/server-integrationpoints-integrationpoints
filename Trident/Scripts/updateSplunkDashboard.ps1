<#
.SYNOPSIS
This script will be used by nightly pipeline to complie and run RelativitySync tests
#>

Get-Location
Get-ChildItem
Write-Host Testtt
Write-Host (Get-Item -Path ".\").FullName
