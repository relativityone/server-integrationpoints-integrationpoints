using kCura.IntegrationPoint.Tests.Core.Models;

namespace kCura.IntegrationPoints.UITests.Tests.ExportToLoadFile
{
	public abstract class ExportToLoadFileTests : UiTest
	{
		protected const string SAVED_SEARCH_NAME = "All Documents";

		protected ExportToLoadFileProviderModel CreateExportToLoadFileProviderModel(string name)
		{
			var model = new ExportToLoadFileProviderModel(name, SAVED_SEARCH_NAME);
			return model;
		}
	}
}
