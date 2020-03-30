using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync
{
	internal sealed class WorkspaceGuidService : IWorkspaceGuidService
	{
		private readonly ISyncServiceManager _servicesManager;
		private readonly IDictionary<int, Guid> _cache;

		public WorkspaceGuidService(ISyncServiceManager servicesManager)
		{
			_servicesManager = servicesManager;
			_cache = new ConcurrentDictionary<int, Guid>();
		}

		public async Task<Guid> GetWorkspaceGuidAsync(int workspaceArtifactId)
		{
			Guid workspaceGuid;

			if (_cache.ContainsKey(workspaceArtifactId))
			{
				workspaceGuid = _cache[workspaceArtifactId];
			}
			else
			{
				workspaceGuid = await ReadWorkspaceGuidAsync(workspaceArtifactId).ConfigureAwait(false);
				_cache.Add(workspaceArtifactId, workspaceGuid);
			}

			return workspaceGuid;
		}

		private async Task<Guid> ReadWorkspaceGuidAsync(int workspaceArtifactId)
		{
			using (IObjectManager objectManager = _servicesManager.CreateProxy<IObjectManager>(ExecutionIdentity.System))
			{
				QueryRequest queryRequest = new QueryRequest()
				{
					ObjectType = new ObjectTypeRef()
					{
						Name = "Workspace"
					},
					Condition = $"'ArtifactID' == {workspaceArtifactId}"
				};
				QueryResult queryResult = await objectManager.QueryAsync(-1, queryRequest, 0, 1).ConfigureAwait(false);

				if (queryResult.Objects.Count == 0)
				{
					throw new NotFoundException($"Workspace ArtifactID = {workspaceArtifactId} not found.");
				}

				return queryResult.Objects.First().Guids.FirstOrDefault();
			}
		}
	}
}