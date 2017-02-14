IF NOT EXISTS (SELECT * FROM [eddsdbo].[TOGGLE] WHERE [Name] = 'kCura.IntegrationPoints.Core.Toggles.RipToR1Toggle')
BEGIN
	INSERT INTO [eddsdbo].[Toggle] VALUES ('kCura.IntegrationPoints.Core.Toggles.RipToR1Toggle', 1)
END