using System.Threading;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.Search;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.Logging;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Integration.Stubs;

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
	public class DestinationWorkspaceSavedSearchCreationStepTests : SystemTest
	{
		private WorkspaceRef _destinationWorkspace;
		private const int _USER_ID = 9;

		[SetUp]
		public async Task SetUp()
		{
			_destinationWorkspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
		}

		[Test]
		public async Task ItShouldCreateSavedSearch()
		{
			// ARRANGE
			const int sourceCaseTagArtifactId = 456;
			const int sourceJobTagArtifactId = 789;
			const int sourceWorkspaceArtifactId = 123;
			string sourceWorkspaceTagName = "Source Workspace Tag Name";
			string sourceJobTagName = "Source Job Tag Name";
			
			ConfigurationStub configuration = new ConfigurationStub
			{
				CreateSavedSearchForTags = true,
				DestinationWorkspaceArtifactId = _destinationWorkspace.ArtifactID,
				SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
				SourceWorkspaceTagArtifactId = sourceCaseTagArtifactId,
				SourceWorkspaceTagName = sourceWorkspaceTagName,
				SourceJobTagArtifactId = sourceJobTagArtifactId,
				SourceJobTagName = sourceJobTagName,
				ExecutingUserId = _USER_ID
			};

			// ACT
			ISyncJob syncJob = CreateSyncJob(configuration);
			await syncJob.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			bool savedSearchExists = await DoesSavedSearchExist(sourceJobTagName).ConfigureAwait(false);

			savedSearchExists.Should().BeTrue();
		}

		private async Task<bool> DoesSavedSearchExist(string sourceJobTagName)
		{
			using (var savedSearchManager = ServiceFactory.CreateProxy<IKeywordSearchManager>())
			{
				Services.Query query = new Services.Query
				{
					Condition = $"\"Name\" == \"{sourceJobTagName}\""
				};
				KeywordSearchQueryResultSet result = await savedSearchManager.QueryAsync(_destinationWorkspace.ArtifactID, query).ConfigureAwait(false);
				return result.Results[0].Artifact != null;
			}
		}

		private ISyncJob CreateSyncJob(ConfigurationStub configuration)
		{
			ContainerBuilder containerBuilder = new ContainerBuilder();

			ContainerFactory factory = new ContainerFactory();
			SyncJobParameters syncParameters = new SyncJobParameters(configuration.JobArtifactId, configuration.SourceWorkspaceArtifactId);
			factory.RegisterSyncDependencies(containerBuilder, syncParameters, new SyncJobExecutionConfiguration(), new EmptyLogger());

			new SystemTestsInstaller().Install(containerBuilder);

			IntegrationTestsContainerBuilder.RegisterExternalDependenciesAsMocks(containerBuilder);
			IntegrationTestsContainerBuilder.MockStepsExcept<IDestinationWorkspaceSavedSearchCreationConfiguration>(containerBuilder);

			containerBuilder.RegisterInstance(configuration).AsImplementedInterfaces();

			return containerBuilder.Build().Resolve<ISyncJob>();
		}
	}
}