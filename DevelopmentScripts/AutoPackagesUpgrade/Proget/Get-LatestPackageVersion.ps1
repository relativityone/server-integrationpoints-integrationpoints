<#
.SYNOPSIS

.DESCRIPTION

.EXAMPLE
#>

[CmdletBinding()]
Param(
	[Parameter(Mandatory=$True)]
	[string]$PackageName
)
Process
{
	Write-Verbose "Beginning of Get-LatestPackageVersion.ps1"
	
	$package = Find-Package $PackageName -Source ProGet
	
	Write-Verbose "End of Get-LatestPackageVersion.ps1"

	$package.Version
}