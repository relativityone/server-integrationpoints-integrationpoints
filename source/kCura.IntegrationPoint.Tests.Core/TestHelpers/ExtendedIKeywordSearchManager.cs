using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.Field;
using Relativity.Services.Search;
using Relativity.Services.User;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
	public class ExtendedIKeywordSearchManager : IKeywordSearchManager
	{
		private readonly ITestHelper _helper;
		private readonly ExecutionIdentity _identity;
		private Lazy<IKeywordSearchManager> _managerWrapper;
		private IKeywordSearchManager Manager => _managerWrapper.Value;

		public ExtendedIKeywordSearchManager(ITestHelper helper, ExecutionIdentity identity)
		{
			_helper = helper;
			_identity = identity;
			_managerWrapper = new Lazy<IKeywordSearchManager>(helper.CreateUserProxy<IKeywordSearchManager>);
		}

		private readonly object _obj = new object();

		public void Dispose()
		{
			lock (_obj)
			{
				Manager.Dispose();
				_managerWrapper = new Lazy<IKeywordSearchManager>(_helper.CreateUserProxy<IKeywordSearchManager>);
			}
		}

		public Task<int> CreateSingleAsync(int workspaceArtifactId, KeywordSearch searchDto)
		{
			return Manager.CreateSingleAsync(workspaceArtifactId, searchDto);
		}

		public Task<KeywordSearch> ReadSingleAsync(int workspaceArtifactId, int searchArtifactId)
		{
			return Manager.ReadSingleAsync(workspaceArtifactId, searchArtifactId);
		}

		public Task UpdateSingleAsync(int workspaceArtifactId, KeywordSearch searchDto)
		{
			return Manager.UpdateSingleAsync(workspaceArtifactId, searchDto);
		}

		public Task<SavedSearchMoveResultSet> MoveAsync(int workspaceArtifactID, int artifactID, int destinationContainerID)
		{
			return Manager.MoveAsync(workspaceArtifactID, artifactID, destinationContainerID);
		}

		public Task<SavedSearchMoveResultSet> MoveAsync(int workspaceArtifactID, int artifactID, int destinationContainerID,
			CancellationToken cancel = new CancellationToken())
		{
			return Manager.MoveAsync(workspaceArtifactID, artifactID, destinationContainerID, cancel);
		}

		public Task<SavedSearchMoveResultSet> MoveAsync(int workspaceArtifactID, int artifactID, int destinationContainerID, IProgress<MoveProcessStateProgress> progress = null)
		{
			return Manager.MoveAsync(workspaceArtifactID, artifactID, destinationContainerID, progress);
		}

		public Task<SavedSearchMoveResultSet> MoveAsync(int workspaceArtifactID, int artifactID, int destinationContainerID,
			CancellationToken cancel = new CancellationToken(), IProgress<MoveProcessStateProgress> progress = null)
		{
			return Manager.MoveAsync(workspaceArtifactID, artifactID, destinationContainerID, cancel, progress);
		}

		public Task DeleteSingleAsync(int workspaceArtifactId, int searchArtifactId)
		{
			return Manager.DeleteSingleAsync(workspaceArtifactId, searchArtifactId);
		}

		public Task<KeywordSearchQueryResultSet> QueryAsync(int workspaceArtifactId, Query query)
		{
			return Manager.QueryAsync(workspaceArtifactId, query);
		}

		public Task<KeywordSearchQueryResultSet> QueryAsync(int workspaceArtifactId, Query query, int length)
		{
			return Manager.QueryAsync(workspaceArtifactId, query, length);
		}

		public Task<KeywordSearchQueryResultSet> QuerySubsetAsync(int workspaceArtifactId, string queryToken, int start, int length)
		{
			return Manager.QuerySubsetAsync(workspaceArtifactId, queryToken, start, length);
		}

		public Task<List<UserRef>> GetSearchOwnersAsync(int workspaceArtifactId)
		{
			return Manager.GetSearchOwnersAsync(workspaceArtifactId);
		}

		public Task<List<FieldRef>> GetSearchIncludesAsync(int workspaceArtifactId)
		{
			return Manager.GetSearchIncludesAsync(workspaceArtifactId);
		}

		public Task<SearchResultViewFields> GetFieldsForSearchResultViewAsync(int workspaceArtifactId, int artifactTypeId)
		{
			return Manager.GetFieldsForSearchResultViewAsync(workspaceArtifactId, artifactTypeId);
		}

		public Task<SearchResultViewFields> GetFieldsForSearchResultViewAsync(int workspaceArtifactId, int artifactTypeId, int searchArtifactId)
		{
			return Manager.GetFieldsForSearchResultViewAsync(workspaceArtifactId, artifactTypeId, searchArtifactId);
		}

		public Task<List<FieldRef>> GetFieldsForObjectCriteriaCollectionAsync(int workspaceArtifactId, FieldRef field, int artifactTypeId)
		{
			return Manager.GetFieldsForObjectCriteriaCollectionAsync(workspaceArtifactId, field, artifactTypeId);
		}

		public Task<List<FieldRef>> GetFieldsForCriteriaConditionAsync(int workspaceArtifactId, int artifactTypeId)
		{
			return Manager.GetFieldsForCriteriaConditionAsync(workspaceArtifactId, artifactTypeId);
		}

		public Task<string> GetEmailToLinkUrlAsync(int workspaceArtifactId, int searchArtifactId)
		{
			return Manager.GetEmailToLinkUrlAsync(workspaceArtifactId, searchArtifactId);
		}

		public Task<List<int>> GetReferencedSavedSearchesAsync(int workspaceArtifactId, int searchArtifactId)
		{
			return Manager.GetReferencedSavedSearchesAsync(workspaceArtifactId, searchArtifactId);
		}

		public Task<SearchAccessStatus> GetAccessStatusAsync(int workspaceArtifactID, int artifactID, List<int> ancestorArtifactIDs)
		{
			return Manager.GetAccessStatusAsync(workspaceArtifactID, artifactID, ancestorArtifactIDs);
		}
	}
}
