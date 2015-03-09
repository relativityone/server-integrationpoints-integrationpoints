IF (NOT EXISTS (SELECT * FROM [EDDSResource].[INFORMATION_SCHEMA].[TABLES] WHERE TABLE_SCHEMA = 'eddsdbo' AND  TABLE_NAME = @tableName))
BEGIN
	declare @table nvarchar(1000) = N'create table [EDDSRESOURCE].[EDDSDBO].[' + @tableName +'] ([JobID] bigint
	CONSTRAINT [PK_' + @tableName + ' ] PRIMARY KEY CLUSTERED 
	(
	[JobID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]';
	execute sp_executesql @table
END

declare @insert nvarchar(150) = 'insert into [EDDSRESOURCE].[EDDSDBO].[' + @tableName + '] values ' + '(@id)';
declare @params nvarchar(50) = N'@id bigint';
EXECUTE sp_executesql @insert, @params, @id = @jobID