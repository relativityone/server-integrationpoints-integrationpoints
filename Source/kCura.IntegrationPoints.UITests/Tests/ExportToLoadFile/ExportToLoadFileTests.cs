using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.UITests.Common;
using NUnit.Framework;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.UITests.Tests.ExportToLoadFile
{
	[Category(TestCategory.WEB_IMPORT_EXPORT)]
	[Feature.DataTransfer.IntegrationPoints.WebExport]
	public abstract class ExportToLoadFileTests : UiTest
	{
		protected const string SAVED_SEARCH_NAME = "All Documents";

		protected ExportToLoadFileTests() : base(shouldImportDocuments: true)
		{ }

		protected ExportToLoadFileProviderModel CreateExportToLoadFileProviderModel(string name)
		{
			var model = new ExportToLoadFileProviderModel(name, SAVED_SEARCH_NAME);
			return model;
		}
	}
}
