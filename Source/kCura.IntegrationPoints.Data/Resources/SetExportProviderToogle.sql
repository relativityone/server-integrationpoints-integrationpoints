IF NOT EXISTS (SELECT * FROM [eddsdbo].[TOGGLE] WHERE [Name] = 'kCura.IntegrationPoints.Web.Toggles.ShowFileShareDataProviderToggle')
BEGIN
	INSERT INTO [eddsdbo].[Toggle] VALUES ('kCura.IntegrationPoints.Web.Toggles.ShowFileShareDataProviderToggle', 1)
END
ELSE
BEGIN
	UPDATE [eddsdbo].[Toggle] SET IsEnabled = 1
	WHERE [Name] = 'kCura.IntegrationPoints.Web.Toggles.ShowFileShareDataProviderToggle'
END