using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Process
{

	public class ExportProcessRunnerTest
	{
		private ExportProcessRunner _objectToTest;
		private ExportSettings _settings;
		private WorkspaceService _service;

		[SetUp]
		public void Init()
		{
			
			_objectToTest = new ExportProcessRunner();
			_service = new WorkspaceService(new Helper());
			_settings = DefaultExportConfigLoader.Create();
		}

		[Test]
		[Ignore("Integration Test")]
		public void it_should_export_saved_search()
		{
			var dt = _service.GetDocumentDataTable();
			_service.ImportDocument(1016890, dt.CreateDataReader());
			//var obj = _service.GetWorkspaceId("Test1");
			//_objectToTest.StartWith(_settings);

		}
	}
}
