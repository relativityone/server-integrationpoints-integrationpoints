using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.Search;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Helpers;

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
	public class DestinationWorkspaceSavedSearchCreationStepTests : SystemTest
	{
		private WorkspaceRef _destinationWorkspace;
		private const string _LOCAL_INSTANCE_NAME = "This Instance";

		[SetUp]
		public async Task SetUp()
		{
			_destinationWorkspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
		}

		[Test]
		public async Task ItShouldCreateSavedSearch()
		{
			// ARRANGE
			const int sourceWorkspaceArtifactId = 123;
			const int jobHistoryArtifactId = 456;
			const int userId = 9;
			string jobHistoryName = "Job History Tag Name";
			string sourceWorkspaceName = "Source Workspace";
			string sourceWorkspaceTagName = "Source Workspace Tag Name";
			string sourceJobTagName = "Source Job Tag Name";

			int sourceCaseTagArtifactId = await CreateRelativitySourceCaseTag(sourceWorkspaceTagName, sourceWorkspaceArtifactId, sourceWorkspaceName).ConfigureAwait(false);
			int sourceJobTagArtifactId = await CreateSourceJobTag(jobHistoryArtifactId, jobHistoryName, sourceCaseTagArtifactId, sourceJobTagName).ConfigureAwait(false);

			ConfigurationStub configuration = new ConfigurationStub
			{
				CreateSavedSearchForTags = true,
				DestinationWorkspaceArtifactId = _destinationWorkspace.ArtifactID,
				SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
				SourceJobTagArtifactId = sourceJobTagArtifactId,
				SourceJobTagName = sourceJobTagName,
				ExecutingUserId = userId
			};
			
			ISyncJob syncJob = SyncJobHelper.CreateWithMockedContainerExceptProvidedType<IDestinationWorkspaceSavedSearchCreationConfiguration>(configuration);
			
			// ACT
			await syncJob.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			bool savedSearchExists = await DoesSavedSearchExist(sourceJobTagName).ConfigureAwait(false);

			savedSearchExists.Should().BeTrue();
		}

		private async Task<int> CreateRelativitySourceCaseTag(string sourceWorkspaceTagName, int sourceWorkspaceArtifactId, string sourceWorkspaceName)
		{
			RelativitySourceCaseTag sourceCaseTag = new RelativitySourceCaseTag
			{
				Name = sourceWorkspaceTagName,
				SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
				SourceWorkspaceName = sourceWorkspaceName,
				SourceInstanceName = _LOCAL_INSTANCE_NAME
			};

			int sourceCaseTagArtifactId = await Rdos.CreateRelativitySourceCaseInstance(ServiceFactory, _destinationWorkspace.ArtifactID, sourceCaseTag).ConfigureAwait(false);
			return sourceCaseTagArtifactId;
		}

		private async Task<int> CreateSourceJobTag(int jobHistoryArtifactId, string jobHistoryName, int sourceCaseTagArtifactId, string sourceJobTagName)
		{
			RelativitySourceJobTag sourceJobTag = new RelativitySourceJobTag
			{
				JobHistoryArtifactId = jobHistoryArtifactId,
				JobHistoryName = jobHistoryName,
				SourceCaseTagArtifactId = sourceCaseTagArtifactId,
				Name = sourceJobTagName
			};

			int sourceJobTagArtifactId = await Rdos.CreateRelativitySourceJobInstance(ServiceFactory, _destinationWorkspace.ArtifactID, sourceJobTag).ConfigureAwait(false);
			return sourceJobTagArtifactId;
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
	}
}