SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

--Do cleanup first - delete old tables (over 24 hours old)
DECLARE @table varchar(255) 
DECLARE @dropCommand varchar(300) 
DECLARE tableCursor CURSOR FOR 
		SELECT QUOTENAME('EDDSResource')+'.'+QUOTENAME(s.name)+'.'+QUOTENAME(t.name) 
		FROM [EDDSResource].[sys].[tables] AS t 
		INNER JOIN [EDDSResource].[sys].[schemas] AS s 
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


IF (EXISTS (SELECT * FROM [EDDSResource].[INFORMATION_SCHEMA].[TABLES] WHERE TABLE_SCHEMA = 'eddsdbo' AND  TABLE_NAME = @tableName))
BEGIN
	declare @sql nvarchar(1000) = N'delete from [EDDSResource].[eddsdbo].[' + @tableName +'] Where [JobID] = @id'
	declare @params nvarchar(50) = N'@id bigint';
	Execute sp_executesql @sql, @params, @id = @jobID


	SET @sql = 'IF EXISTS(select [JobID] FROM [EDDSResource].[eddsdbo].['+ @tableName+'])
	BEGIN
		select 1
	END
	ELSE
	BEGIN
		drop table [EddsResource].[eddsdbo].[' + @tableName +']
		select 0
	END'
	EXECUTE sp_executesql @sql
END
ELSE
BEGIN
	select 1
END