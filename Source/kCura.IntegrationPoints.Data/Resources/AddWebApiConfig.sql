IF NOT EXISTS (SELECT * FROM [eddsdbo].[Configuration] WHERE [Section] = 'kCura.IntegrationPoints' AND [Name] = 'WebAPIPath')
BEGIN
	INSERT INTO [eddsdbo].[Configuration] VALUES ('kCura.IntegrationPoints', 'WebAPIPath', 'http://localhost/RelativityWebAPI/', '', 'Relativity WebAPI URL for Relativity Integration Points')
END
ELSE
BEGIN
	UPDATE	[eddsdbo].[Configuration] 
	SET			[Description] = 'Relativity WebAPI URL for Relativity Integration Points'
	WHERE		[Section] = 'kCura.IntegrationPoints' AND [Name] = 'WebAPIPath'
END