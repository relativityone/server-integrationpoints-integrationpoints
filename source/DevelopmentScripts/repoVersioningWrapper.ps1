<#
.SYNOPSIS
	Passes the correct values into teamcityVersioningWrapper.ps1
.DESCRIPTION
	Looks at root\Version\version.txt for the major & minor version numbers, then passes those along with the ProductName,
	ServerInstance, and Database into teamcityVersioningWrapper.ps1
.EXAMPLE
	.\repoVersioningWrapper.ps1 -ProductName
.NOTES
	Author: David Kirk
	Date:   14 September, 2016
#>
param(
	[parameter(Mandatory=$true)] $ProductName,
	[parameter()] $ServerInstance = "BLD-MSTR-01.kcura.corp",
	[parameter()] $Database = "BuildVersion"
)

$root = (git rev-parse --show-toplevel).Replace("/", "\")
$versionFile = [System.IO.Path]::Combine($root, 'Version\version.txt')

if (Test-Path $versionFile)
{
	$Major = (Get-Content $versionFile).split(".")[0]
	$Minor = (Get-Content $versionFile).split(".")[1]

	.\teamcityVersioningWrapper.ps1 -ProductName $ProductName -Major $Major -Minor $Minor -ServerInstance $ServerInstance -Database $Database
}

else {
	Write-Host "Error:" $versionFile "must be defined"
}