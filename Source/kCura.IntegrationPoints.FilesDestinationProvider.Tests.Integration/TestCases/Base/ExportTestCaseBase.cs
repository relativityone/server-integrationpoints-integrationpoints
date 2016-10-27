using System.Data;
using System.IO;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Abstract;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Model;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base
{
	public abstract class ExportTestCaseBase : IExportTestCase
	{
		protected ExportSettings ExportSettings { get; private set; }

		public virtual ExportSettings Prepare(ExportSettings settings)
		{
			settings.ExportFilesLocation += $"_{GetType().Name}";

			ExportSettings = settings;

			return ExportSettings;
		}

		public abstract void Verify(DirectoryInfo directory, DocumentsTestData documentsTestData);
	}
}
