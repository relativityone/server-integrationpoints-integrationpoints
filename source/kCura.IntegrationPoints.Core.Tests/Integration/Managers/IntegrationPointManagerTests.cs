using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Integration.Managers
{
	[Explicit]
	public class IntegrationPointManagerTests : WorkspaceDependentTemplate
	{
		private IIntegrationPointService _integrationPointService;

		public IntegrationPointManagerTests() : base("IntegrationPointManagerSource", "IntegrationPointManagerTarget")
		{

		}

		[TestFixtureSetUp]
		public override void SetUp()
		{
			base.SetUp();
			ResolveServices();
		}

		[Test]
		[Explicit]
		public void ExampleTest()
		{
			//Arrange

			IntegrationModel integrationModel = new IntegrationModel
			{
				Destination = CreateDefaultDestinationConfig(),
				DestinationProvider = DestinationProvider.ArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = "IntegrationPoint" + DateTime.Now,
				SelectedOverwrite = "Append Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				Map = CreateDefaultFieldMap()
			};

			//Create Integration Point

			//Great Group and User
			int groupId = Group.CreateGroup("Smoke Test Group");
			bool addedGroupToTargetWorkspace = Group.AddGroupToWorkspace(TargetWorkspaceArtifactId, groupId);
			bool addedGroupToSourceWorkspace = Group.AddGroupToWorkspace(SourceWorkspaceArtifactId, groupId);

			UserModel user = User.CreateUser("New", "Test", "permissionsTest@kcura.com", new[] { groupId });

			//Assign permissions here using Permissions.cs or any private methods

			//Act
			
			//Execute whatever action you are verifying permissions under the context of the user
			SharedVariables.RelativityUserName = user.EmailAddress;
			SharedVariables.RelativityPassword = user.Password;

			IntegrationModel integrationPointCreated = CreateOrUpdateIntegrationPoint(integrationModel); //Creation Example
			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactId, integrationPointCreated.ArtifactID, user.ArtifactId); //Run example
			_integrationPointService.RetryIntegrationPoint(SourceWorkspaceArtifactId, integrationPointCreated.ArtifactID, user.ArtifactId); // retry example

			//Assert


		}

		private void ResolveServices()
		{
			_integrationPointService = Container.Resolve<IIntegrationPointService>();
		}
	}
	
}
