using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.DataContracts.DTOs.Search;
using Relativity.Services.Search;
using Relativity.Services.User;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
	/// <summary>
	///     The most useless wrapper ever!
	/// </summary>
	internal sealed class ExtendedSearchContainerManager : ISearchContainerManager
	{
		private readonly ITestHelper _helper;
		private ExecutionIdentity _executionIdentity;
		private Lazy<ISearchContainerManager> _managerWrapper;
		private ISearchContainerManager Manager => _managerWrapper.Value;

		public ExtendedSearchContainerManager(ITestHelper helper, ExecutionIdentity executionIdentity)
		{
			_helper = helper;
			_executionIdentity = executionIdentity;
			_managerWrapper = new Lazy<ISearchContainerManager>(helper.CreateUserProxy<ISearchContainerManager>);
		}

		private readonly object _lock = new object();

		public void Dispose()
		{
			lock (_lock)
			{
				Manager.Dispose();
				_managerWrapper = new Lazy<ISearchContainerManager>(_helper.CreateUserProxy<ISearchContainerManager>);
			}
		}

		public async Task<int> CreateSingleAsync(int workspaceArtifactID, SearchContainer searchContainer)
		{
			return await Manager.CreateSingleAsync(workspaceArtifactID, searchContainer).ConfigureAwait(false);
		}

		public Task UpdateSingleAsync(int workspaceArtifactID, SearchContainer searchContainer)
		{
			throw new NotImplementedException();
		}

		public Task DeleteSingleAsync(int workspaceArtifactID, int searchContainerArtifactID)
		{
			throw new NotImplementedException();
		}

		public Task<SearchContainerQueryResultSet> QueryAsync(int workspaceArtifactID, Query query, int length)
		{
			throw new NotImplementedException();
		}

		public async Task<SearchContainerQueryResultSet> QueryAsync(int workspaceArtifactID, Query query)
		{
			return await Manager.QueryAsync(workspaceArtifactID, query).ConfigureAwait(false);
		}

		public Task<SearchContainerQueryResultSet> QuerySubsetAsync(int workspaceArtifactID, string queryToken, int start, int length)
		{
			throw new NotImplementedException();
		}

		public Task<SearchContainer> ReadSingleAsync(int workspaceArtifactID, int searchContainerArtifactID)
		{
			throw new NotImplementedException();
		}

		public Task<SearchContainerItemCollection> GetSearchContainerItemsAsync(int workspaceArtifactID, SearchContainerRef searchContainer)
		{
			throw new NotImplementedException();
		}

		public Task<SearchContainerItemCollection> GetChildSearchContainersAsync(int workspaceArtifactID, SearchContainerRef searchContainer)
		{
			throw new NotImplementedException();
		}

		public Task<SearchContainerItemCollection> GetSearchContainerTreeAsync(int workspaceArtifactID, List<int> expandedNodes)
		{
			throw new NotImplementedException();
		}

		public Task<SearchContainerItemCollection> GetSearchContainerTreeAsync(int workspaceArtifactID, List<int> expandedNodes, int? selectedNode)
		{
			throw new NotImplementedException();
		}

		public Task<SearchContainerItemCollection> GetFilteredSearchContainerTreeAsync(int workspaceArtifactId, string searchCondition)
		{
			throw new NotImplementedException();
		}

		public Task<SearchContainerItemCollection> GetFilteredWithAdvancedOptionsSearchContainerTreeAsync(int workspaceArtifactID, SearchContainerTreeFilter filter)
		{
			throw new NotImplementedException();
		}

		public Task<SearchContainerMoveResultSet> MoveAsync(int workspaceArtifactID, int artifactID, int destinationContainerID)
		{
			throw new NotImplementedException();
		}

		public Task<SearchContainerMoveResultSet> MoveAsync(int workspaceArtifactID, int artifactID, int destinationContainerID, CancellationToken cancel)
		{
			throw new NotImplementedException();
		}

		public Task<SearchContainerMoveResultSet> MoveAsync(int workspaceArtifactID, int artifactID, int destinationContainerID, IProgress<MoveProcessStateProgress> progress)
		{
			throw new NotImplementedException();
		}

		public Task<SearchContainerMoveResultSet> MoveAsync(int workspaceArtifactID, int artifactID, int destinationContainerID, CancellationToken cancel, IProgress<MoveProcessStateProgress> progress)
		{
			throw new NotImplementedException();
		}

		public Task<AdvancedSearchViewInfo> GetAdvancedSearchViewInfoAsync(int workspaceArtifactID)
		{
			throw new NotImplementedException();
		}

		public Task<List<UserRef>> GetAdvancedSearchViewUniqueOwnersAsync(int workspaceArtifactID)
		{
			throw new NotImplementedException();
		}

		public Task<List<UserRef>> GetAdvancedSearchViewUniqueCreatedByAsync(int workspaceArtifactID)
		{
			throw new NotImplementedException();
		}

		public Task<List<UserRef>> GetAdvancedSearchViewUniqueModifiedByAsync(int workspaceArtifactID)
		{
			throw new NotImplementedException();
		}
	}
}