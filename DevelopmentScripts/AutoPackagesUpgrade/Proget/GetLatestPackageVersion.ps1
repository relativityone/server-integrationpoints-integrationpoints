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
	Write-Verbose "Beginning of GetLatestPackageVersion.ps1"
	
	$package = Find-Package $PackageName -Source ProGet
	
	Write-Verbose "End of GetLatestPackageVersion.ps1"

	return $package.Version
}