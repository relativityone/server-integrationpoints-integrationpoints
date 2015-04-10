IF (NOT EXISTS (SELECT * FROM [EDDSResource].[INFORMATION_SCHEMA].[TABLES] WHERE TABLE_SCHEMA = 'eddsdbo' AND  TABLE_NAME = @tableName))
BEGIN
	declare @table nvarchar(1000) = N'create table [EDDSRESOURCE].[EDDSDBO].[' + @tableName +'] 
	([JobID] bigint,
	[TotalRecords] int,
	[ErrorRecords] int,
	[Completed] bit,
	CONSTRAINT [PK_' + @tableName + ' ] PRIMARY KEY CLUSTERED 
	(
	[JobID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]';
	execute sp_executesql @table
	declare @insert nvarchar(500) = 'insert into [EDDSRESOURCE].[EDDSDBO].[' + @tableName + '] ([JobID],[Completed]) values (@id, 0)';
	declare @params nvarchar(50) = N'@id bigint';
	EXECUTE sp_executesql @insert, @params, @id = @jobID
END


declare @update nvarchar(max) = '
UPDATE [EDDSRESOURCE].[EDDSDBO].[' + @tableName + ']
	SET
		[TotalRecords] = @total,
		[ErrorRecords] = @errored
	WHERE
		[JobId] = @id

SELECT
	SUM(COALESCE([TotalRecords],0)) as [TotalRecords],
	SUM(COALESCE([ErrorRecords],0)) as [ErrorRecords]
FROM [EDDSRESOURCE].[EDDSDBO].[' + @tableName + ']';
DECLARE @uParams nvarchar(max) = N'@id bigint, @total bigint, @errored bigint';
EXECUTE sp_executesql @update, @uParams, @id = @jobID, @total = @total, @errored = @errored
