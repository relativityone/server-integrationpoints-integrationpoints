using System.Threading;
using System.Threading.Tasks;
using Autofac;
using NUnit.Framework;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.Logging;
using Relativity.Sync.Tests.Integration;
using Relativity.Sync.Tests.Integration.Stubs;
using Relativity.Sync.Tests.System.Helpers;
using Relativity.Sync.Tests.System.Stubs;

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
	public sealed class ValidationExecutorSystemTests : SystemTest
	{
		private WorkspaceRef _destinationWorkspace;
		private WorkspaceRef _sourceWorkspace;

		private const string _JOB_HISTORY_NAME = "Test Job Name";
		private const int _USER_ID = 9;

		[SetUp]
		public async Task SetUp()
		{
			Task<WorkspaceRef> sourceWorkspaceCreationTask = Environment.CreateWorkspaceWithFieldsAsync();
			Task<WorkspaceRef> destinationWorkspaceCreationTask = Environment.CreateWorkspaceAsync();
			await Task.WhenAll(sourceWorkspaceCreationTask, destinationWorkspaceCreationTask).ConfigureAwait(false);
			_sourceWorkspace = sourceWorkspaceCreationTask.Result;
			_destinationWorkspace = destinationWorkspaceCreationTask.Result;
		}

		[Test]
		public async Task ItShouldSuccessfulyValidateJob()
		{
			int expectedSourceWorkspaceArtifactId = _sourceWorkspace.ArtifactID;
			int expectedJobHistoryArtifactId = await Rdos.CreateJobHistoryInstance(ServiceFactory, expectedSourceWorkspaceArtifactId, _JOB_HISTORY_NAME).ConfigureAwait(false);
			int savedSearchArtifactId = await Rdos.GetSavedSearchInstance(ServiceFactory, expectedSourceWorkspaceArtifactId).ConfigureAwait(false);
			int destinationFolderArtifactId = await Rdos.GetRootFolderInstance(ServiceFactory, _destinationWorkspace.ArtifactID).ConfigureAwait(false);
			int folderPathSourceFieldArtifactId = await Rdos.GetFolderPathSourceField(ServiceFactory, expectedSourceWorkspaceArtifactId).ConfigureAwait(false);

			const string fieldsMap = "[{\"sourceField\":{\"displayName\":\"Control Number [Object Identifier]\",\"isIdentifier\":true," +
							"\"fieldIdentifier\":\"1003667\",\"isRequired\":true},\"destinationField\":" +
							"{\"displayName\":\"Control Number [Object Identifier]\",\"isIdentifier\":true,\"fieldIdentifier\":\"1003667\"," +
							"\"isRequired\":true},\"fieldMapType\":\"Identifier\"}]";

			ConfigurationStub configuration = new ConfigurationStub
			{
				DestinationWorkspaceArtifactId = _destinationWorkspace.ArtifactID,
				SourceWorkspaceArtifactId = expectedSourceWorkspaceArtifactId,
				JobArtifactId = expectedJobHistoryArtifactId,
				JobName = _JOB_HISTORY_NAME,
				ExecutingUserId = _USER_ID,
				NotificationEmails = string.Empty,
				SavedSearchArtifactId = savedSearchArtifactId,
				DestinationFolderArtifactId = destinationFolderArtifactId,
				FieldMappings = fieldsMap,
				FolderPathSourceFieldArtifactId = folderPathSourceFieldArtifactId,
				ImportOverwriteMode = ImportOverwriteMode.AppendOverlay,
				DestinationFolderStructureBehavior = DestinationFolderStructureBehavior.ReadFromField,
				FieldOverlayBehavior = FieldOverlayBehavior.UseFieldSettings
			};

			// act
			ISyncJob syncJob = CreateSyncJob(configuration);

			// assert
			await syncJob.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);
		}

		private ISyncJob CreateSyncJob(ConfigurationStub configuration)
		{
			ContainerBuilder containerBuilder = new ContainerBuilder();

			ContainerFactory factory = new ContainerFactory();
			SyncJobParameters syncParameters = new SyncJobParameters(configuration.JobArtifactId, configuration.SourceWorkspaceArtifactId);
			factory.RegisterSyncDependencies(containerBuilder, syncParameters, new SyncJobExecutionConfiguration(), new EmptyLogger());

			new SystemTestsInstaller().Install(containerBuilder);

			IntegrationTestsContainerBuilder.RegisterExternalDependenciesAsMocks(containerBuilder);
			IntegrationTestsContainerBuilder.MockStepsExcept<IValidationConfiguration>(containerBuilder);

			containerBuilder.RegisterInstance(configuration).AsImplementedInterfaces();

			return containerBuilder.Build().Resolve<ISyncJob>();
		}
	}
}