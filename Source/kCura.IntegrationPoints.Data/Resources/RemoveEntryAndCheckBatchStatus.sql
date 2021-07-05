SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

--Do cleanup first - delete old tables (over 72 hours old)
DECLARE @table varchar(255) 
DECLARE @dropCommand varchar(300) 
DECLARE tableCursor CURSOR FOR 
		SELECT '{0}.' + QUOTENAME(t.name) 
		FROM {1}.[sys].[tables] AS t 
		INNER JOIN {1}.[sys].[schemas] AS s 
		ON t.[schema_id] = s.[schema_id] 
		WHERE DATEDIFF(HOUR,t.create_date,GETUTCDATE())>72
		AND t.name LIKE 'RIP_JobTracker_%'

OPEN tableCursor 
FETCH next FROM tableCursor INTO @table 

WHILE @@fetch_status=0 
BEGIN 
		SET @dropcommand = N'DROP TABLE ' + @table 
		--PRINT(@dropcommand) 
		EXECUTE(@dropcommand) 
		FETCH next FROM tableCursor INTO @table 
END 

CLOSE tableCursor 
DEALLOCATE tableCursor


IF OBJECT_ID(N'{0}.[{2}]',N'U') IS NOT NULL
BEGIN
	IF @batchIsFinished = 1
	BEGIN
		UPDATE {0}.[{2}] SET [Completed] = 1 WHERE [JobID] = @jobID
	END
	
	IF EXISTS(select [JobID] FROM {0}.[{2}] WHERE [Completed] = 0)
	BEGIN
		SELECT 1
	END
	ELSE
	BEGIN
		DROP TABLE {0}.[{2}]
		SELECT 0
	END
END
ELSE
BEGIN
	SELECT 1
END