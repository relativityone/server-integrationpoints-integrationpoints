using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.Services.Objects;
using Relativity.Sync.Configuration;
using Relativity.Sync.Tests.Common.RdoGuidProviderStubs;
using Relativity.Sync.Tests.System.Core.Extensions;
using AppSettings = Relativity.Sync.Tests.System.Core.AppSettings;

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

		/// <summary>
		/// Creates gold flow tests run.
		/// </summary>
		/// <param name="configureAsync">Method used to set up configuration.</param>
		/// <param name="destinationWorkspaceID">Artifact ID of the existing destination workspace, or null to create new workspace.</param>
		public async Task<IGoldFlowTestRun> CreateTestRunAsync(Func<WorkspaceRef, WorkspaceRef, ConfigurationStub, Task> configureAsync, int? destinationWorkspaceID = null)
		{
			WorkspaceRef destinationWorkspace;

			if (destinationWorkspaceID.HasValue)
			{
				destinationWorkspace = await _environment.GetWorkspaceAsync(destinationWorkspaceID.Value).ConfigureAwait(false);
			}
			else
			{
				destinationWorkspace = await _environment.CreateWorkspaceAsync(templateWorkspaceName: SourceWorkspace.Name).ConfigureAwait(false);
			}

			ConfigurationStub configuration = new ConfigurationStub
			{
				CreateSavedSearchForTags = false
			};

			configuration.SetEmailNotificationRecipients(string.Empty);

			configuration.SourceWorkspaceArtifactId = SourceWorkspace.ArtifactID;
			configuration.DestinationWorkspaceArtifactId = destinationWorkspace.ArtifactID;
			configuration.SavedSearchArtifactId = await Rdos.GetSavedSearchInstanceAsync(ServiceFactory, SourceWorkspace.ArtifactID).ConfigureAwait(false);
			configuration.DataSourceArtifactId = configuration.SavedSearchArtifactId;
			configuration.JobHistoryArtifactId = await Rdos.CreateJobHistoryInstanceAsync(ServiceFactory, SourceWorkspace.ArtifactID, $"Sync Job {DateTime.Now:yyyy MMMM dd HH.mm.ss.fff}").ConfigureAwait(false);
			configuration.DestinationFolderArtifactId = await Rdos.GetRootFolderInstanceAsync(ServiceFactory, destinationWorkspace.ArtifactID).ConfigureAwait(false);

			await configureAsync(SourceWorkspace, destinationWorkspace, configuration).ConfigureAwait(false);

			configuration.SyncStatisticsId = await Rdos.CreateEmptySyncStatisticsRdoAsync(SourceWorkspace.ArtifactID).ConfigureAwait(false);

			int configurationId = await Rdos.CreateSyncConfigurationRdoAsync(SourceWorkspace.ArtifactID, configuration)
				.ConfigureAwait(false);

			return new GoldFlowTestRun(this, configurationId, configuration);
		}

		internal interface IGoldFlowTestRun
		{
			int DestinationWorkspaceArtifactId { get; }

			Task<SyncJobState> RunAsync();

			Task AssertAsync(SyncJobState result, int expectedItemsTransferred, int expectedTotalItems, Guid? jobHistoryGuid = null);
			void AssertDocuments(string[] sourceDocumentsNames, string[] destinationDocumentsNames);
			Task AssertImagesAsync(int sourceWorkspaceId, RelativityObject[] sourceWorkspaceDocuments, int destinationWorkspaceId, RelativityObject[] destinationWorkspaceDocuments);
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
				_parameters = new SyncJobParameters(configurationId, goldFlowTestSuite.SourceWorkspace.ArtifactID, Guid.NewGuid());
			}

			public Task<SyncJobState> RunAsync()
			{
				var syncRunner = new SyncRunner(new ServicesManagerStub(), AppSettings.RelativityUrl, new NullAPM(), TestLogHelper.GetLogger());

				return syncRunner.RunAsync(_parameters, _goldFlowTestSuite.User.ArtifactID);
			}

			public async Task AssertAsync(SyncJobState result, int expectedItemsTransferred, int expectedTotalItems, Guid? jobHistoryGuid = null)
			{
				result.Status.Should().Be(SyncJobStatus.Completed, result.Message);
				jobHistoryGuid = jobHistoryGuid ?? DefaultGuids.JobHistory.TypeGuid;

				RelativityObject jobHistory = await Rdos
					.GetJobHistoryAsync(_goldFlowTestSuite.ServiceFactory, _goldFlowTestSuite.SourceWorkspace.ArtifactID, _configuration.JobHistoryArtifactId, jobHistoryGuid.Value)
					.ConfigureAwait(false);
				int itemsTransferred = (int)jobHistory["Items Transferred"].Value;
				int totalItems = (int) jobHistory["Total Items"].Value;

				using (var objectManager = _goldFlowTestSuite.ServiceFactory.CreateProxy<IObjectManager>())
				{
					string aggregatedJobHistoryErrors =
						await objectManager.AggregateJobHistoryErrorMessagesAsync(_goldFlowTestSuite.SourceWorkspace.ArtifactID, jobHistory.ArtifactID).ConfigureAwait(false);

					aggregatedJobHistoryErrors.Should().BeNullOrEmpty("There should be no item level errors");
				}

				itemsTransferred.Should().Be(expectedItemsTransferred);
				totalItems.Should().Be(expectedTotalItems);
			}
			
			public void AssertDocuments(string[] sourceDocumentsNames, string[] destinationDocumentsNames)
			{
				var destinationDocumentsNamesSet = new HashSet<string>(destinationDocumentsNames);

				foreach (var name in sourceDocumentsNames)
				{
					destinationDocumentsNamesSet.Contains(name).Should().BeTrue($"Document {name} was not created in destination workspace");
				}
			}

			public async Task AssertImagesAsync(int sourceWorkspaceId, RelativityObject[] sourceWorkspaceDocuments, int destinationWorkspaceId, RelativityObject[] destinationWorkspaceDocuments)
			{
				string GetExpectedIdentifier(string controlNumber, int index)
				{
					if (index == 0)
					{
						return controlNumber;
					}

					return $"{controlNumber}_{index}";
				}

				IEnumerable<TestImageFile> sourceWorkspaceFiles;
				Dictionary<string, TestImageFile> destinationWorkspaceFiles;

				Dictionary<int, string> sourceWorkspaceDocumentNames = sourceWorkspaceDocuments.ToDictionary(x => x.ArtifactID, x => x.Name);

				using (ISearchService searchService = _goldFlowTestSuite.ServiceFactory.CreateProxy<ISearchService>())
				{
					sourceWorkspaceFiles = await GetWorkspaceImageFilesAsync(
						searchService, sourceWorkspaceId, sourceWorkspaceDocuments).ConfigureAwait(false);

					destinationWorkspaceFiles = (await GetWorkspaceImageFilesAsync(searchService,
							destinationWorkspaceId, destinationWorkspaceDocuments).ConfigureAwait(false))
						.ToDictionary(x => x.Identifier, x => x);
				}

				foreach (IGrouping<int, TestImageFile> sourceDocumentImages in sourceWorkspaceFiles.ToLookup(x => x.DocumentArtifactId, x => x))
				{
					int i = 0;
					foreach (TestImageFile imageFile in sourceDocumentImages)
					{
						string expectedIdentifier = GetExpectedIdentifier(sourceWorkspaceDocumentNames[imageFile.DocumentArtifactId], i);

						destinationWorkspaceFiles.ContainsKey(expectedIdentifier).Should()
							.BeTrue($"Image [{sourceDocumentImages.Key} => {expectedIdentifier}] was not pushed to destination workspace");

						TestImageFile destinationImage = destinationWorkspaceFiles[expectedIdentifier];

						TestImageFile.AssertAreEquivalent(imageFile, destinationImage, expectedIdentifier);

						if (_configuration.ImportImageFileCopyMode == ImportImageFileCopyMode.SetFileLinks)
						{
							TestImageFile.AssertImageIsLinked(imageFile, destinationImage);
						}

						i++;
					}
				}
			}

			private async Task<IEnumerable<TestImageFile>> GetWorkspaceImageFilesAsync(
				ISearchService searchService, int workspaceId, RelativityObject[] workspaceDocumentIds)
			{
				DataSetWrapper dataSet = await searchService.RetrieveImagesForSearchAsync(workspaceId,
						workspaceDocumentIds.Select(x => x.ArtifactID).ToArray(), Guid.Empty.ToString())
					.ConfigureAwait(false);

				return dataSet.Unwrap().Tables[0]
					.AsEnumerable()
					.Select(TestImageFile.GetFile);
			}
		}
	}
}
