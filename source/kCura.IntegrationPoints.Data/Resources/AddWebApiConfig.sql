IF NOT EXISTS (SELECT * FROM [eddsdbo].[Configuration] WHERE [Section] = 'kCura.Relativity.IntegrationPoints' AND [Name] = 'WebAPIPath')
BEGIN
	insert into [eddsdbo].[Configuration] ([Section], [Name], [Value],  [MachineName], [Description])
	SELECT TOP 1 
		'kCura.Relativity.IntegrationPoints' as [Section],
		'WebAPIPath' as [Name],
		value as [Value],
		'' as [MachineName],
		'The URL for the Windows Authenticated Relativity Web API endpoint used by integration points.' as [Description]
	 FROM
	 (SELECT CASE WHEN [Section] = 'kCura.EDDS.DBMT' THEN 2
		else 1 end as prime, 
		coalesce(value,'')as value
	from [eddsdbo].[Configuration]
	WHERE 
		([Section] = 'kCura.EDDS.DBMT' AND [Name] = 'WebAPIPath')
		OR
		([Section] = 'Relativity.Core'	AND	[Name] = 'ProcessingWebAPIPath' )
	) t1
	where  LTRIM(RTRIM(value)) <>''
	ORDER BY t1.prime
END