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
		AND t.name LIKE 'RIP_CustodianManager_%'

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


IF NOT EXISTS (SELECT * FROM EDDSResource.sys.objects WHERE object_id = OBJECT_ID(N'[EDDSResource].[eddsdbo].[{0}]') AND type in (N'U'))
BEGIN
	CREATE TABLE [EDDSResource].[eddsdbo].[{0}](
		[ID] [bigint] IDENTITY(1,1) NOT NULL,
		[CustodianID] [nvarchar](1000) NOT NULL,
		[ManagerID] [nvarchar](1000) NOT NULL,
		[LockedByJobID] [bigint] NULL,
		[CreatedOn] [datetime] NOT NULL,
		CONSTRAINT [PK_{0}] PRIMARY KEY CLUSTERED 
		(
			[ID] ASC
		)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]
END
