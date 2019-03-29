using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Logging;
using Serilog;
using Serilog.Events;

namespace kCura.IntegrationPoints.UITests.Configuration.Helpers
{
	internal class ImportDocumentsHelper
	{
		private readonly TestContext _testContext;

		private static readonly ILogger Log = LoggerFactory.CreateLogger(typeof(ImportDocumentsHelper));

		public ImportDocumentsHelper(TestContext testContext)
		{
			_testContext = testContext;
		}

		public void ImportDocuments(bool withNatives, DocumentTestDataBuilder.TestDataType testDataType)
		{
			Log.Information(@"Importing documents...");
			string testDir = NUnit.Framework.TestContext.CurrentContext.TestDirectory.Replace("kCura.IntegrationPoints.UITests",
				"kCura.IntegrationPoint.Tests.Core");
			Log.Information("TestDir for ImportDocuments '{testDir}'", testDir);
			DocumentsTestData data = DocumentTestDataBuilder.BuildTestData(testDir, withNatives, testDataType);
			var importHelper = new ImportHelper();
			var workspaceService = new WorkspaceService(importHelper);
			bool importSucceded = workspaceService.ImportData(_testContext.GetWorkspaceId(), data);
			if (!importSucceded)
			{
				string suffix = importHelper.ErrorMessages.Any()
					? " Error messages: " + string.Join("; ", importHelper.ErrorMessages)
					: " No error messages.";
				throw new UiTestException("Import of documents failed." + suffix);
			}

			if (Log.IsEnabled(LogEventLevel.Verbose))
			{
				string suffix = importHelper.Messages.Any() ? " Messages: " + string.Join("; ", importHelper.Messages) : " No messages.";
				Log.Verbose(@"Documents imported." + suffix);
			}
		}
	}
}
