<#
.SYNOPSIS
	Calls out to stored procedure on SQL server to get and increment Assembly & Installer version numbers
.DESCRIPTION
	Pass in parameters ProductName, Major, Minor, ServerInstance, Database
.EXAMPLE
	Import-Module .\getAndIncrementVersion.ps1
	getAndIncrementVersion -ProductName 'Relativity' -Major 9 -Minor 4 -ServerInstance "BLD-MSTR-01.kcura.corp" -Database "BuildVersion"
.NOTES
	Author: David Kirk
	Date:   7 September, 2016
#>

function getAndIncrementVersion {
	param(
		[parameter(Mandatory=$true)] $ProductName,
		[parameter(Mandatory=$true)] $Major,
		[parameter(Mandatory=$true)] $Minor,
		[parameter(Mandatory=$true)] $ServerInstance,
		[parameter(Mandatory=$true)] $Database
	)

	$query = @"
		DECLARE @AssemblyVersion varchar(50), @InstallerVersion varchar(50)
		EXEC getAndIncrementVersionNumbers @ProductName = '{0}', @Major = {1}, @Minor = {2}, @AssemblyVersion = @AssemblyVersion OUTPUT, @InstallerVersion = @InstallerVersion OUTPUT
		SELECT @AssemblyVersion as 'AssemblyVersion', @InstallerVersion as 'InstallerVersion'
"@ 		-f $ProductName, $Major, $Minor
	
	$connectionString = ('Server={0};Database={1};User=Version;Password=Test1234!') -f $ServerInstance,$Database
	return Read-Query -ConnectionString $connectionString -Query $query
}

#Read-Query function taken from http://www.22bugs.co/post/simple-alternative-to-invoke-sqlcmd/
function Read-Query {
	param (
		[Parameter(Mandatory=$true)] $ConnectionString,
		[Parameter(Mandatory=$true)] $Query
	)

	$SqlConnection = New-Object System.Data.SqlClient.SqlConnection
	$SqlConnection.ConnectionString = $ConnectionString
	$SqlConnection.Open()
	$SqlCmd = New-Object System.Data.SqlClient.SqlCommand
	$SqlCmd.CommandText = $Query
	$SqlCmd.Connection = $SqlConnection
	$reader = $SqlCmd.ExecuteReader()

	$ret = @{}

	while ($reader.Read())
	{
		for ($i = 0; $i -lt $reader.FieldCount; ++$i)
		{
			$ret.add($reader.GetName($i), $reader[$i])
		}
	}

	$SqlConnection.Close()

	return $ret
}

Export-ModuleMember -function getAndIncrementVersion

