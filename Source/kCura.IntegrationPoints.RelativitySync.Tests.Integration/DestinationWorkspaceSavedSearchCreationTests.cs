using System;
using System.Threading;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.RelativitySync.Adapters;
using kCura.IntegrationPoints.RelativitySync.Tests.Integration.Stubs;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Search;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.RelativitySync.Tests.Integration
{
	internal sealed class DestinationWorkspaceSavedSearchCreationTests : RelativityProviderTemplate
	{
		private DestinationWorkspaceSavedSearchCreation _instance;
		private DestinationWorkspaceSavedSearchCreationConfigurationStub _configuration;

		private int _workspaceTagArtifactId;
		private int _jobTagArtifactId;
		private string _jobTagName;


		public DestinationWorkspaceSavedSearchCreationTests() : base(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
		{
		}

		public override void TestSetup()
		{
			base.TestSetup();

			IWindsorContainer container = new WindsorContainer();
			container.Register(Component.For<IHelper>().Instance(Helper));
			container.Register(Component.For<IAPILog>().Instance(new Mock<IAPILog>().Object));

			ConfigureWorkspace(container).ConfigureAwait(false).GetAwaiter().GetResult();

			_instance = new DestinationWorkspaceSavedSearchCreation(container);
			_configuration = new DestinationWorkspaceSavedSearchCreationConfigurationStub
			{
				DestinationWorkspaceArtifactId = TargetWorkspaceArtifactId,
				SourceJobTagArtifactId = _jobTagArtifactId,
				SourceJobTagName = _jobTagName,
				SourceWorkspaceTagArtifactId = _workspaceTagArtifactId,
				CreateSavedSearchForTags = true
			};
		}

		private async Task ConfigureWorkspace(IWindsorContainer container)
		{
			IntegrationPointModel integrationPointModel = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOverlay, "name", "Append Only");
			integrationPointModel = CreateOrUpdateIntegrationPoint(integrationPointModel);

			JobHistory jobHistory = CreateJobHistoryOnIntegrationPoint(integrationPointModel.ArtifactID, Guid.NewGuid(), JobTypeChoices.JobHistoryRun);

			DestinationWorkspaceObjectTypesCreation objectTypesCreation = new DestinationWorkspaceObjectTypesCreation(container);
			DestinationWorkspaceObjectTypesCreationConfigurationStub objectTypesConfiguration = new DestinationWorkspaceObjectTypesCreationConfigurationStub
			{
				DestinationWorkspaceArtifactId = TargetWorkspaceArtifactId
			};
			await objectTypesCreation.ExecuteAsync(objectTypesConfiguration, CancellationToken.None).ConfigureAwait(false);

			DestinationWorkspaceTagsCreation tagsCreation = new DestinationWorkspaceTagsCreation(container);
			DestinationWorkspaceTagsCreationConfigurationStub tagsConfiguration = new DestinationWorkspaceTagsCreationConfigurationStub
			{
				SourceWorkspaceArtifactId = SourceWorkspaceArtifactId,
				DestinationWorkspaceArtifactId = TargetWorkspaceArtifactId,
				SourceWorkspaceArtifactTypeId = objectTypesConfiguration.SourceWorkspaceArtifactTypeId,
				SourceJobArtifactTypeId = objectTypesConfiguration.SourceJobArtifactTypeId,
				JobArtifactId = jobHistory.ArtifactId
			};
			await tagsCreation.ExecuteAsync(tagsConfiguration, CancellationToken.None).ConfigureAwait(false);

			_workspaceTagArtifactId = tagsConfiguration.SourceWorkspaceTagArtifactId.Value;
			_jobTagArtifactId = tagsConfiguration.SourceJobTagArtifactId.Value;
			_jobTagName = tagsConfiguration.SourceJobTagName;
		}

		[IdentifiedTest("021e8fd2-0eda-45cc-bca1-86387dea63ae")]
		public async Task ItShouldCreateSavedSearch()
		{
			// ACT
			await _instance.ExecuteAsync(_configuration, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			Assert.IsTrue(_configuration.IsSavedSearchArtifactIdSet);

			using (IKeywordSearchManager searchManager = Helper.GetServicesManager().CreateProxy<IKeywordSearchManager>(ExecutionIdentity.System))
			{
				KeywordSearch result = await searchManager.ReadSingleAsync(TargetWorkspaceArtifactId, _configuration.SavedSearchArtifactId).ConfigureAwait(false);

				Assert.NotNull(result);
			}
		}

		[IdentifiedTest("dbad434f-c37e-4ea1-8103-c6d7ab1fb37a")]
		public async Task ItShouldNotExecuteIfNotCreatingSavedSearch()
		{
			_configuration.CreateSavedSearchForTags = false;
			_configuration.SetSavedSearchArtifactId(1);

			// ACT
			bool result = await _instance.CanExecuteAsync(_configuration, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			Assert.IsFalse(result);
		}

		[IdentifiedTest("719adaf1-ea49-4c2f-bdb8-5513f26d2663")]
		public async Task ItShouldNotExecuteIfSavedSearchAlreadyCreated()
		{
			_configuration.SetSavedSearchArtifactId(1);

			// ACT
			bool result = await _instance.CanExecuteAsync(_configuration, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			Assert.IsFalse(result);
		}

		[IdentifiedTest("8bf2a0a9-63a8-4e4a-8eb7-5f406738c7e8")]
		public async Task ItShouldExecuteIfCreatingSavedSearchAndItDoesNotExist()
		{
			// ACT
			bool result = await _instance.CanExecuteAsync(_configuration, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			Assert.IsTrue(result);
		}
	}
}