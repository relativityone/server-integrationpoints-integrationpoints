<#
.SYNOPSIS
	Wraps the getAndIncrementVersion module so that TeamCity is properly passed the correct results
.DESCRIPTION
	Pass in parameters ProductName, Major, Minor, ServerInstance, Database
.EXAMPLE
	.\teamcityVersioningWrapper.ps1 -ProductName 'Relativity' -Major 9 -Minor 4 -ServerInstance "BLD-MSTR-01.kcura.corp" -Database "BuildVersion"
.NOTES
	Author: David Kirk
	Date:   7 September, 2016
#>

param(
	[parameter(Mandatory=$true)] $ProductName,
	[parameter(Mandatory=$true)] $Major,
	[parameter(Mandatory=$true)] $Minor,
	[parameter()] $ServerInstance = "BLD-MSTR-01.kcura.corp",
	[parameter()] $Database = "BuildVersion"
)

Import-Module .\getAndIncrementVersion.psm1 -Force

$versionContainer = (getAndIncrementVersion -ProductName $ProductName -Major $Major -Minor $Minor -ServerInstance $ServerInstance -Database $Database)

$assemblyVersionOutput = "##teamcity[setParameter name='env.assemblyVersionNumber' value='{0}']" -f ($versionContainer.AssemblyVersion)
$installerVersionOutput = "##teamcity[setParameter name='env.installerVersionNumber' value='{0}']" -f ($versionContainer.InstallerVersion)
$teamCityBuildNumberOutput = "##teamcity[buildNumber '{0}']" -f ($versionContainer.AssemblyVersion)

Write-Host $assemblyVersionOutput
Write-Host $installerVersionOutput
Write-Host $teamCityBuildNumberOutput

return $versionContainer