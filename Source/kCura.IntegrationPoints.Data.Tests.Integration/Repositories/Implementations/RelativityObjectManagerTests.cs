using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Data.Tests.Integration.Repositories.Implementations
{
	[TestFixture]
	public class RelativityObjectManagerTests
	{
		private int _workspaceId;
		private ImportHelper _importHelper;
		private WorkspaceService _workspaceService;

		private const string _WORKSPACE_NAME = "RIP_StreamLongTextAsyncIntegrationTests";

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_workspaceId = Workspace.CreateWorkspace(_WORKSPACE_NAME, SourceProviderTemplate.WorkspaceTemplates.NEW_CASE_TEMPLATE);
			_importHelper = new ImportHelper();
			_workspaceService = new WorkspaceService(_importHelper);
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			Workspace.DeleteWorkspace(_workspaceId);
		}

        [Test]
		public void StreamLongTextAsync_ItShouldFetchDocumentWith15MBExtractedText()
        {
	        long bytes15MB = (long) (15 * 1024 * 1024);
	        _workspaceService.ImportExtractedTextSimple(_workspaceId, bytes15MB);
			Assert.Ignore();
        }

	}
}
