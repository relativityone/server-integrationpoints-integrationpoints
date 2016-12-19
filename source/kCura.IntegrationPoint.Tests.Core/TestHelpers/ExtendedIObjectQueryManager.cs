using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.DataContracts.DTOs;
using Relativity.Services.ObjectQuery;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
	public class ExtendedIObjectQueryManager : IObjectQueryManager
	{
		private readonly ITestHelper _helper;
		private readonly ExecutionIdentity _identity;
		private Lazy<IObjectQueryManager> _managerWrapper;
		private IObjectQueryManager Manager => _managerWrapper.Value;

		public ExtendedIObjectQueryManager(ITestHelper helper, ExecutionIdentity identity)
		{
			_helper = helper;
			_identity = identity;
			_managerWrapper = new Lazy<IObjectQueryManager>(helper.CreateUserProxy<IObjectQueryManager>);
		}

		private readonly object _lock = new object();

		public void Dispose()
		{
			lock (_lock)
			{
				// create a new Kepler when itself being disposed.
				Manager.Dispose();
				_managerWrapper = new Lazy<IObjectQueryManager>(_helper.CreateUserProxy<IObjectQueryManager>);
			}
		}

		public Task<ObjectQueryResultSet> QueryAsync(int workspaceId, int artifactTypeId, Query query, int start, int length, int[] includePermissions, string queryToken)
		{
			return Manager.QueryAsync(workspaceId, artifactTypeId, query, start, length, includePermissions, queryToken);
		}

		public Task<ObjectQueryResultSet> QueryAsync(int workspaceId, int artifactTypeId, Query query, int start, int length, int[] includePermissions, string queryToken, IProgress<ProgressReport> progress)
		{
			return Manager.QueryAsync(workspaceId, artifactTypeId, query, start, length, includePermissions, queryToken, progress);
		}

		public Task<ObjectQueryResultSet> QueryAsync(int workspaceId, int artifactTypeId, Query query, int start, int length, int[] includePermissions, string queryToken, CancellationToken cancel)
		{
			return Manager.QueryAsync(workspaceId, artifactTypeId, query, start, length, includePermissions, queryToken, cancel);
		}

		public Task<ObjectQueryResultSet> QueryAsync(int workspaceId, int artifactTypeId, Query query, int start, int length, int[] includePermissions, string queryToken, CancellationToken cancel, IProgress<ProgressReport> progress)
		{
			return Manager.QueryAsync(workspaceId, artifactTypeId, query, start, length, includePermissions, queryToken, cancel, progress);
		}

		public Task<ObjectQueryUniqueFieldValuesResult> QueryUniqueFieldValuesAsync(int workspaceId, int artifactTypeId, string fieldName, CancellationToken cancel,
			IProgress<ProgressReport> progress)
		{
			return Manager.QueryUniqueFieldValuesAsync(workspaceId, artifactTypeId, fieldName, cancel, progress);
		}
	}
}