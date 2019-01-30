[CmdletBinding(DefaultParameterSetName='fromVersionFile')]
param (
	[Parameter(Mandatory)]
	[string] $Product,

	[Parameter(Mandatory)]
	[string] $Project,
	
	[Parameter(ParameterSetName='fromVersionFile')]
	[string] $VersionFilePath,

	[Parameter(ParameterSetName='manualVersion')]
	[int] $MajorVersion,

	[Parameter(ParameterSetName='manualVersion')]
	[int] $MinorVersion,

	[string] $BuildType = 'DEV',

	[string] $ServerType = 'local',

	[string] $ServerInstance = 'BLD-MSTR-01.kcura.corp',

	[string] $Database = 'TCBuildVersion'
)

process
{
	if ($PSCmdlet.ParameterSetName -eq 'fromVersionFile')
	{
		if ($VersionFilePath -and (Test-Path $VersionFilePath))
		{
			$versionRaw = Get-Content $VersionFilePath
			$MajorVersion = $versionRaw.split(".")[0]
			$MinorVersion = $versionRaw.split(".")[1]
		}
		else
		{
			$alternateVersionFilePath = Join-Path $PSScriptRoot '..\Version\version.txt'
			if ($VersionFilePath)
			{
				Write-Warning "Could not find version file '$VersionFilePath'; attempting to find in: '$alternateVersionFilePath'"
			}

			if (Test-Path $alternateVersionFilePath)
			{
				$versionRaw = Get-Content $alternateVersionFilePath
				$MajorVersion = $versionRaw.split(".")[0]
				$MinorVersion = $versionRaw.split(".")[1]
			}
			else
			{
				throw "Could not find version file"
			}
		}
	}

	$conn = New-Object System.Data.SqlClient.SqlConnection
	$conn.ConnectionString = "server='$ServerInstance';Database='$Database';user=StoryboardUser;password=Test1234!;"

	$conn.Open()

	try
	{
		$command = New-Object System.Data.SqlClient.SqlCommand
		$command.Connection = $conn
		$command.CommandText = "
		SET NOCOUNT ON
				
		--Declare variables used by SQL passed in from NANT script
		DECLARE @productName AS VARCHAR(255) = '$Product'
		DECLARE @projectName AS VARCHAR(255) = '$Project'
		DECLARE @majorversion AS VARCHAR(10) = '$MajorVersion'
		DECLARE @minorversion AS VARCHAR(10) = '$MinorVersion'
		DECLARE @branchID AS INT


		--Insert the product/project into the table if it doesn't already exist
		IF ('$ServerType' <> 'local' AND NOT EXISTS(SELECT 1 FROM TCBuildSemanticVersion WHERE ProductName = @productName AND ProjectName = @projectName AND Major = @majorversion AND Minor = @minorversion)) 
		BEGIN
			INSERT INTO TCBuildSemanticVersion (ProductName, ProjectName, Major, Minor, Patch, Build)
			VALUES (@productName, @projectName, @majorversion, @minorversion, 0, 0)
		END


		SET @branchID = (SELECT TOP 1 [ID]
						FROM TCBuildSemanticVersion
						WHERE ProductName = @productName 
						AND ProjectName = @projectName
						AND Major = @majorversion 
						AND Minor = @minorversion)


		--Select the version of the build
		SELECT CAST(Major AS VARCHAR) + '.' + CAST(Minor AS VARCHAR) + '.' + CAST(Patch AS VARCHAR) + '.' + CAST(Build AS VARCHAR) AS BuildVersion
		FROM TCBuildSemanticVersion
		WHERE [ID] = @branchID

		--Increment Patch if GOLD build
		IF('$ServerType' <> 'local' AND '$BuildType' = 'GOLD')
			BEGIN
				UPDATE TCBuildSemanticVersion 
				SET Patch = Patch + 1,
					Build = 0 
				WHERE [ID] = @branchID
			END
		ELSE
			BEGIN
			--Increment Build so next build will be a later version
			IF('$ServerType' <> 'local')
				BEGIN
					UPDATE TCBuildSemanticVersion 
					SET Build = Build + 1 
					WHERE [ID] = @branchID
				END
			END

		"

		$command.ExecuteScalar()
	}
	finally
	{
		$conn.Close()
	}
}
