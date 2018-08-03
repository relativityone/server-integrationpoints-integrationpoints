SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

--Do cleanup first - delete old tables (over 72 hours old)
DECLARE @table varchar(255) 
DECLARE @dropCommand varchar(300) 

DECLARE tableCursor CURSOR FOR 
		SELECT '{0}.'+QUOTENAME(t.name) 
		FROM {1}.[sys].[tables] AS t 
		INNER JOIN {1}.[sys].[schemas] AS s 
		ON t.[schema_id] = s.[schema_id] 
		WHERE DATEDIFF(HOUR,t.create_date,GETUTCDATE())>72
		AND t.name LIKE 'RIP_EntityManager_%'

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


IF OBJECT_ID(N'{0}.[{2}]',N'U') IS NULL
BEGIN
	CREATE TABLE {0}.[{2}](
		[ID] [bigint] IDENTITY(1,1) NOT NULL,
		[EntityID] [nvarchar](1000) NOT NULL,
		[ManagerID] [nvarchar](1000) NOT NULL,
		[LockedByJobID] [bigint] NULL,
		[CreatedOn] [datetime] NOT NULL,
		CONSTRAINT [PK_{2}] PRIMARY KEY CLUSTERED 
		(
			[ID] ASC
		)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]
END
