IF NOT EXISTS (SELECT * FROM [eddsdbo].[TOGGLE] WHERE [Name] = 'kCura.IntegrationPoints.Web.Toggles.UI.ShowImageImportToggle')
BEGIN
	INSERT INTO [eddsdbo].[Toggle] VALUES ('kCura.IntegrationPoints.Web.Toggles.UI.ShowImageImportToggle', 1)
END