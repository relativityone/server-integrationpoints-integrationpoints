using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.DataContracts.DTOs;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors
{
	internal sealed class WorkspaceNameQuery : IWorkspaceNameQuery
	{
		private readonly ISyncLog _logger;

		public WorkspaceNameQuery(ISyncLog logger)
		{
			_logger = logger;
		}

		public async Task<string> GetWorkspaceNameAsync(IProxyFactory proxyFactory, int workspaceArtifactId, CancellationToken token)
		{
			using (IObjectManager objectManager = await proxyFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				QueryRequest request = new QueryRequest()
				{
					Condition = $"'ArtifactID' == {workspaceArtifactId}",
					ObjectType = new ObjectTypeRef() { ArtifactTypeID = (int)ArtifactType.Case },
					IncludeNameInQueryResult = true
				};
				const int adminWorkspaceId = -1;
				const int start = 0;
				const int length = 1;
				QueryResult queryResult;

				try
				{
					queryResult = await objectManager.QueryAsync(adminWorkspaceId, request, start, length, token, new EmptyProgress<ProgressReport>()).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Failed to query for workspace Artifact ID: {workspaceArtifactId}", workspaceArtifactId);
					throw;
				}

				if (!queryResult.Objects.Any())
				{
					_logger.LogError("Couldn't find workspace Artifact ID: {workspaceArtifactId}", workspaceArtifactId);
					throw new SyncException($"Couldn't find workspace Artifact ID: {workspaceArtifactId}");
				}

				return queryResult.Objects.First().Name;
			}
		}
	}
}