using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Data.Tests.Integration.Repositories
{
	[TestFixture]
	public class KeplerWorkspaceRepositoryTests
	{
		private static readonly string _workspaceName = "Integartion Test" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
		private int _workspaceId;
		private ITestHelper _helper;

		[SetUp]
		public void SetUp()
		{
			_workspaceId = Workspace.CreateWorkspace(_workspaceName, SourceProviderTemplate.WorkspaceTemplates.NEW_CASE_TEMPLATE);
			_helper = new TestHelper();
		}

		[TearDown]
		public void TearDown()
		{
			Workspace.DeleteWorkspace(_workspaceId);
		}

		[Test]
		public void Active_Workspace_Returned()
		{
			//Arrange
			IRepositoryFactory repositoryFactory = new RepositoryFactory(_helper, _helper.GetServicesManager());
			IWorkspaceRepository repository = repositoryFactory.GetWorkspaceRepository();
			
			//Act
			IEnumerable<WorkspaceDTO> actualWorkspaces = repository.RetrieveAllActive();

			//Assert
			Assert.That(actualWorkspaces.Any(x => x.ArtifactId == _workspaceId));
		}
	}
}
