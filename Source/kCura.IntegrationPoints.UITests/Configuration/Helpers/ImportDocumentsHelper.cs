﻿using System.Diagnostics;
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
			Log.Information(@"Importing documents... {testDataType} into workspaceId {workspaceId}", testDataType, _testContext.WorkspaceId);
			var sw = new Stopwatch();
			sw.Start();
			string testDir = NUnit.Framework.TestContext.CurrentContext.TestDirectory.Replace("kCura.IntegrationPoints.UITests",
				"kCura.IntegrationPoint.Tests.Core");
			Log.Information("TestDir for ImportDocuments '{testDir}'", testDir);
			string prefix = string.Empty;
			DocumentsTestData data = DocumentTestDataBuilder.BuildTestData(prefix, testDir, withNatives, testDataType);
			var importHelper = new ImportHelper();
			var workspaceService = new WorkspaceService(importHelper);
			workspaceService.ImportData(_testContext.GetWorkspaceId(), data);

			if (Log.IsEnabled(LogEventLevel.Verbose))
			{
				string suffix = importHelper.Messages.Any() ? " Messages: " + string.Join("; ", importHelper.Messages) : " No messages.";
				Log.Verbose(@"Documents imported." + suffix);
			}
			sw.Stop();
			Log.Information("Documents imported. Duration {duration} s", sw.ElapsedMilliseconds/1000);
		}
	}
}
