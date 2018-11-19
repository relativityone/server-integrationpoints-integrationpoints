#Requires -Version 5.0

<#
.SYNOPSIS
    Gets version with incrementing build number

.PARAMETER buildType
    Current build type

.PARAMETER majorNumber
    Current major number

.PARAMETER minorNumber
    Current minor number
#>

[CmdletBinding()]
param(
    [ValidateSet("DEV", "GOLD")]
    [string]$buildType,
    [string]$majorNumber,
    [string]$minorNumber
)

$project = "Relativity.Sync"

$Conn = New-Object System.Data.SqlClient.SqlConnection
$Conn.ConnectionString = "server='bld-mstr-01.kcura.corp';Database='TCBuildVersion';user=StoryboardUser;password=Test1234!;"

try {
    $Conn.Open()

    $Comm = New-Object System.Data.SqlClient.SqlCommand
    $Comm.Connection = $Conn
    $Comm.CommandText = "
SET NOCOUNT ON

--Declare variables used by SQL passed in from NANT script
DECLARE @productName AS VARCHAR(255) = '$project'
DECLARE @majorversion AS VARCHAR(10) = '$majorNumber'
DECLARE @minorversion AS VARCHAR(10) = '$minorNumber'
DECLARE @buildType AS VARCHAR(10) = '$buildType'
DECLARE @branchID AS INT


--Insert the product/project into the table if it doesn't already exist
IF (NOT EXISTS(SELECT 1
                FROM [TCBuildSemanticVersion]
                WHERE [ProductName] = @productName AND [ProjectName] = @productName AND [Major] = @majorversion AND [Minor] = @minorversion))
BEGIN
    INSERT INTO [TCBuildSemanticVersion]
        ([ProductName], [ProjectName], [Major], [Minor], [Patch], [Build])
    VALUES
        (@productName, @productName, @majorversion, @minorversion, 0, 0)
END

SET @branchID = (SELECT TOP 1
    [ID]
FROM [TCBuildSemanticVersion]
WHERE [ProductName] = @productName
    AND [Major] = @majorversion
    AND [Minor] = @minorversion)


--Select the version of the build
SELECT CAST([Major] AS VARCHAR) + '.' + CAST([Minor] AS VARCHAR) + '.' + CAST([Patch] AS VARCHAR) + '.' + CAST([Build] AS VARCHAR) AS BuildVersion
FROM [TCBuildSemanticVersion]
WHERE [ID] = @branchID


IF(@buildType = 'GOLD')
    BEGIN
    --Increment Patch if GOLD build
    UPDATE [TCBuildSemanticVersion] 
        SET [Patch] = [Patch] + 1,
            [Build] = 0 
        WHERE [ID] = @branchID
END
ELSE
BEGIN
    --Increment Build so next build will be a later version
    UPDATE TCBuildSemanticVersion 
            SET Build = Build + 1 
            WHERE [ID] = @branchID

END

"

    $global:nextVersion = $Comm.ExecuteScalar()
    $Conn.Close()
}
finally {
    if ($Conn) {
        $Conn.Close()
    }
}         

if (!$global:nextVersion) {
    throw "Unable to retrieve version from SQL"
}