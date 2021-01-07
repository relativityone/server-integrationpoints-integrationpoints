using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using NUnit.Framework;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.Data.Tests.Integration.Repositories
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	public class KeplerWorkspaceRepositoryTests
	{
		private int _workspaceID;
		private ITestHelper _helper;
		private static readonly string _workspaceName = $"Integration Test {DateTime.UtcNow.ToString("yyyyMMdd_HHmmss")}";

		[SetUp]
		public async Task SetUp()
		{
			_workspaceID = (await Workspace.CreateWorkspaceAsync(_workspaceName)).ArtifactID;
			_helper = new TestHelper();
		}

		[TearDown]
		public async Task TearDown()
		{
			await Workspace.DeleteWorkspaceAsync(_workspaceID).ConfigureAwait(false);
		}

		[IdentifiedTest("3b791349-9028-4126-9835-0dba48f55112")]
		public void Active_Workspace_Returned()
		{
			//Arrange
			IRepositoryFactory repositoryFactory = new RepositoryFactory(_helper, _helper.GetServicesManager());
			IWorkspaceRepository repository = repositoryFactory.GetWorkspaceRepository();
			
			//Act
			IEnumerable<WorkspaceDTO> actualWorkspaces = repository.RetrieveAllActive();

			//Assert
			Assert.That(actualWorkspaces.Any(x => x.ArtifactId == _workspaceID));
		}
	}
}
