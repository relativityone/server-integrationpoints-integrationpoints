using System;
using System.Threading.Tasks;
using Relativity.Telemetry.APM;
using Relativity.Services.Workspace;
using Relativity.Services.ServiceProxy;
using User = Relativity.Services.User.User;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Stubs;
using Relativity.Sync.Tests.System.Core.Runner;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Services.Objects.DataContracts;
using FluentAssertions;

namespace Relativity.Sync.Tests.System.GoldFlows
{
	internal class GoldFlowTestSuite
	{
		private readonly TestEnvironment _environment;

		public User User { get; }

		public ServiceFactory ServiceFactory { get; }

		public WorkspaceRef SourceWorkspace { get; }
		
		private GoldFlowTestSuite(TestEnvironment environment, User user, ServiceFactory serviceFactory, WorkspaceRef sourceWorkspace)
		{
			_environment = environment;
			User = user;
			ServiceFactory = serviceFactory;
			SourceWorkspace = sourceWorkspace;
		}

		internal static async Task<GoldFlowTestSuite> CreateAsync(TestEnvironment environment, User user, ServiceFactory serviceFactory)
		{
			WorkspaceRef sourceWorkspace = await environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
			return new GoldFlowTestSuite(environment, user, serviceFactory, sourceWorkspace);
		}

		public Task ImportDocumentsAsync(ImportDataTableWrapper importDataTable)
		{
			ImportHelper importHelper = new ImportHelper(ServiceFactory);
			return importHelper.ImportDataAsync(SourceWorkspace.ArtifactID, importDataTable);
		}

		public async Task<IGoldFlowTestRun> CreateTestRunAsync(Func<WorkspaceRef, WorkspaceRef, ConfigurationStub, Task> configureAsync)
		{
			WorkspaceRef destinationWorkspace = await _environment.CreateWorkspaceAsync(templateWorkspaceName: SourceWorkspace.Name).ConfigureAwait(false);

			ConfigurationStub configuration = new ConfigurationStub
			{
				CreateSavedSearchForTags = false
			};

			configuration.SetEmailNotificationRecipients(string.Empty);

			configuration.SourceWorkspaceArtifactId = SourceWorkspace.ArtifactID;
			configuration.DestinationWorkspaceArtifactId = destinationWorkspace.ArtifactID;
			configuration.SavedSearchArtifactId = await Rdos.GetSavedSearchInstance(ServiceFactory, SourceWorkspace.ArtifactID).ConfigureAwait(false);
			configuration.DataSourceArtifactId = configuration.SavedSearchArtifactId;
			configuration.JobHistoryArtifactId = await Rdos.CreateJobHistoryInstanceAsync(ServiceFactory, SourceWorkspace.ArtifactID, $"Sync Job {DateTime.Now:yyyy MMMM dd HH.mm.ss.fff}").ConfigureAwait(false);
			configuration.DestinationFolderArtifactId = await Rdos.GetRootFolderInstance(ServiceFactory, destinationWorkspace.ArtifactID).ConfigureAwait(false);

			await configureAsync(SourceWorkspace, destinationWorkspace, configuration).ConfigureAwait(false);

			int configurationId = await Rdos.CreateSyncConfigurationRdoAsync(ServiceFactory, SourceWorkspace.ArtifactID, configuration)
				.ConfigureAwait(false);

			return new GoldFlowTestRun(this, configurationId, configuration);
		}

		internal interface IGoldFlowTestRun
		{
			int DestinationWorkspaceArtifactId { get; }

			Task<SyncJobState> RunAsync();

			Task AssertAsync(SyncJobState result, int expectedItemsTransferred, int expectedTotalItems);
		}

		private class GoldFlowTestRun : IGoldFlowTestRun
		{
			private readonly GoldFlowTestSuite _goldFlowTestSuite;
			private readonly ConfigurationStub _configuration;
			private readonly SyncJobParameters _parameters;

			public int DestinationWorkspaceArtifactId => _configuration.DestinationWorkspaceArtifactId;

			public GoldFlowTestRun(GoldFlowTestSuite goldFlowTestSuite, int configurationId, ConfigurationStub configuration)
			{
				_goldFlowTestSuite = goldFlowTestSuite;
				_configuration = configuration;
				_parameters = new SyncJobParameters(configurationId, goldFlowTestSuite.SourceWorkspace.ArtifactID, _configuration.JobHistoryArtifactId);
			}

			public Task<SyncJobState> RunAsync()
			{
				var syncRunner = new SyncRunner(new ServicesManagerStub(), AppSettings.RelativityUrl, new NullAPM(), TestLogHelper.GetLogger());

				return syncRunner.RunAsync(_parameters, _goldFlowTestSuite.User.ArtifactID);
			}

			public async Task AssertAsync(SyncJobState result, int expectedItemsTransferred, int expectedTotalItems)
			{
				result.Status.Should().Be(SyncJobStatus.Completed, result.Message);

				RelativityObject jobHistory = await Rdos
					.GetJobHistoryAsync(_goldFlowTestSuite.ServiceFactory, _goldFlowTestSuite.SourceWorkspace.ArtifactID, _configuration.JobHistoryArtifactId)
					.ConfigureAwait(false);
				int itemsTransferred = (int)jobHistory["Items Transferred"].Value;
				int totalItems = (int) jobHistory["Total Items"].Value;
				
				itemsTransferred.Should().Be(expectedItemsTransferred);
				totalItems.Should().Be(expectedTotalItems);
			}
		}
	}
}
