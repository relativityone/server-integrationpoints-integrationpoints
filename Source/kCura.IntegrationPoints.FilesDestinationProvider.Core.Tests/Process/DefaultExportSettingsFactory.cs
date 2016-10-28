using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Process
{
	internal class DefaultExportSettingsFactory
	{
		public static ExportSettings Create()
		{
			return new ExportSettings
			{
				SavedSearchName = string.Empty,
				ViewName = string.Empty,
				ProductionName = string.Empty
			};
		}
	}
}