. .\psake-common.ps1


task default -depends getversion


task getversion {
   $Conn = New-Object System.Data.SqlClient.SqlConnection
   $Conn.ConnectionString = "server='$server';Database='$database';user=StoryboardUser;password=Test1234!;"

   $Conn.Open()

   $Comm = New-Object System.Data.SqlClient.SqlCommand
   $Comm.Connection = $Conn
   $Comm.CommandText = "
SET NOCOUNT ON
		
--Declare variables used by SQL passed in from NANT script
DECLARE @productName AS VARCHAR(255) = '$product'
DECLARE @branchName AS VARCHAR(255) = '$branch'
DECLARE @version AS VARCHAR(10) = '$majorminorversion'
DECLARE @startDate AS DATE = '$devstartdate'
DECLARE @branchID AS INT

--Build the table if it doesn't exist
IF (NOT EXISTS(SELECT 1 FROM sys.tables WHERE name = 'TCBuildVersion'))
BEGIN
CREATE TABLE TCBuildVersion
(
	[ID] INT IDENTITY PRIMARY KEY CLUSTERED,
	ProductName VARCHAR(255),
	BranchName VARCHAR(255),
	MajorMinorVersion VARCHAR(10),
	SubVersion INT NULL,
	DevelopmentStartDate DATE,
	BuildOfTheDay INT,
	LastBuild DATE
)
END


--If branch name begins with release then we only keep the release-#.# without the -critical at the end. 
IF @branchName like 'release-%-%'
BEGIN
SET @branchName = (SELECT SUBSTRING(@branchName, 1, CHARINDEX('-', @branchName, 9) - 1))
END


--Insert the project/branch into the table if it doesn't already exist
IF ('$server_type' <> 'local' AND NOT EXISTS(SELECT 1 FROM TCBuildVersion WHERE ProductName = @productName AND BranchName = @branchName)) 
BEGIN
	INSERT INTO TCBuildVersion (ProductName, BranchName, MajorMinorVersion, SubVersion, DevelopmentStartDate, BuildOfTheDay, LastBuild)
	VALUES (@productName, @branchName, @version, NULL, @startDate, 0, '1/1/00')
END


SET @branchID = (SELECT TOP 1 [ID]
                 FROM TCBuildVersion
				 WHERE ProductName = @productName 
				   AND BranchName = @branchName)

--Update majorminor version and dev start date if changed
IF('$server_type' <> 'local' AND EXISTS(SELECT 1 FROM TCBuildVersion WHERE [ID] = @branchID AND (MajorMinorVersion <> @version OR DevelopmentStartDate <> @startDate)))
BEGIN
	UPDATE TCBuildVersion 
	SET MajorMinorVersion = @version,
	    DevelopmentStartDate = @startDate,
		SubVersion = NULL
	WHERE [ID] = @branchID
END

--If the number of days since development started is more than 600, we set the SubVersion column to 600, and we will no longer calculate based on days since development started. 
IF('$server_type' <> 'local' AND EXISTS(SELECT 1 FROM TCBuildVersion WHERE [ID] = @branchID AND SubVersion IS NULL AND DATEDIFF(day, DevelopmentStartDate, GETDATE()) > 600 ))
BEGIN
	UPDATE TCBuildVersion 
	SET SubVersion = 600
	WHERE [ID] = @branchID
END

--If the SubVersion column is not null (more than 600 days passed since development started) we will only change the last build date
IF('$server_type' <> 'local' AND EXISTS(SELECT 1 FROM TCBuildVersion WHERE [ID] = @branchID AND SubVersion IS NOT NULL))
BEGIN
	UPDATE TCBuildVersion 
	SET LastBuild = GETDATE() 
	WHERE [ID] = @branchID
END
ELSE --If the SubVersion is null (less the 600 days have passed since development started) then we change the build of the day back to 1 when the last build date is not today. Set it to day.
IF('$server_type' <> 'local' AND EXISTS(SELECT 1 FROM TCBuildVersion WHERE [ID] = @branchID AND DATEDIFF(day, LastBuild,GETDATE()) <> 0 ))
BEGIN
	UPDATE TCBuildVersion 
	SET BuildOfTheDay = 1, LastBuild = GETDATE() 
	WHERE [ID] = @branchID
END


--Select the version of the build
SELECT MajorMinorVersion 
      + '.' + 
      CASE WHEN SubVersion IS NOT NULL THEN                              --use the subversion if it's not null (more than 600 days of development passed)
	      CAST(SubVersion AS VARCHAR)
	  ELSE                                                               --calculate the number of days since development started, use it was the 3rd version 
	      CAST(DATEDIFF(day, DevelopmentStartDate, GETDATE()) AS VARCHAR) 
	  END 
	  + '.' + 
	  CAST(BuildOfTheDay AS VARCHAR)  AS BuildVersion
FROM TCBuildVersion
WHERE [ID] = @branchID

--Increment BuildOfTheDay so next build will be a later version
IF('$server_type' <> 'local')
BEGIN
    UPDATE TCBuildVersion 
    SET BuildOfTheDay = BuildOfTheDay + 1 
    WHERE [ID] = @branchID
END

--If the build gets beyond 99 for projects where the SubVersion is not null (more than 600 days have passed since development started) then we increment the sub version and reset the build of the day back to 1
--If a build with SubVersion that is null (less than 600 days have passed) and the build of the day reaches 99 it will reset it back to version 1 of the day. Please don't build more than 99 times in one day. 
IF('$server_type' <> 'local' AND EXISTS(SELECT 1 FROM TCBuildVersion WHERE [ID] = @branchID AND BuildOfTheDay > 99))
BEGIN
    UPDATE TCBuildVersion 
    SET BuildOfTheDay = 1, SubVersion = SubVersion + 1
    WHERE [ID] = @branchID
END
"

   $script:version = $Comm.ExecuteScalar()
         
   $Conn.Close()
  
   Write-Host "##teamcity[buildNumber '$script:version']"

   if($server_type -eq 'local'){
    [System.IO.File]::WriteAllText([System.IO.Path]::Combine($development_scripts_directory, 'version.txt'), $script:version)
   }
}





