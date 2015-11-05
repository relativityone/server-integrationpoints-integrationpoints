IF NOT EXISTS (SELECT * FROM [eddsdbo].[Configuration] WHERE [Section] = 'kCura.IntegrationPoints' AND [Name] = 'WebAPIPath')
BEGIN
	insert into [eddsdbo].[Configuration] ([Section], [Name], [Value],  [MachineName], [Description])
	SELECT TOP 1 
		'kCura.IntegrationPoints' as [Section],
		'WebAPIPath' as [Name],
		value as [Value],
		'' as [MachineName],
		'Relativity WebAPI URL for Relativity Integration Points' as [Description]
	FROM	[eddsdbo].[Configuration]
	WHERE 
				[Section] = 'kCura.EDDS.DBMT' 
		AND 
				[Name] = 'WebAPIPath'
END
ELSE
BEGIN
	UPDATE	[eddsdbo].[Configuration] 
	SET			[Description] = 'Relativity WebAPI URL for Relativity Integration Points'
	WHERE		[Section] = 'kCura.IntegrationPoints' AND [Name] = 'WebAPIPath'
END