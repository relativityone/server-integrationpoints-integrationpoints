using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;

namespace Relativity.Sync
{
    internal sealed class WorkspaceGuidService : IWorkspaceGuidService, IDisposable
    {
        private readonly ISourceServiceFactoryForAdmin _serviceFactoryForAdmin;
        private readonly IDictionary<int, Guid> _cache;
        private readonly ISemaphoreSlim _semaphoreSlim;

        public WorkspaceGuidService(ISourceServiceFactoryForAdmin serviceFactoryForAdmin, ISemaphoreSlim semaphoreSlim)
        {
            _serviceFactoryForAdmin = serviceFactoryForAdmin;
            _semaphoreSlim = semaphoreSlim;
            _cache = new ConcurrentDictionary<int, Guid>();
        }

        public async Task<Guid> GetWorkspaceGuidAsync(int workspaceArtifactId)
        {
            Guid workspaceGuid;

            await _semaphoreSlim.WaitAsync();
            try
            {
                if (_cache.ContainsKey(workspaceArtifactId))
                {
                    workspaceGuid = _cache[workspaceArtifactId];
                }
                else
                {
                    workspaceGuid = await ReadWorkspaceGuidAsync(workspaceArtifactId).ConfigureAwait(false);
                    _cache.Add(workspaceArtifactId, workspaceGuid);
                }
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            return workspaceGuid;
        }

        private async Task<Guid> ReadWorkspaceGuidAsync(int workspaceArtifactId)
        {
            using (IObjectManager objectManager = await _serviceFactoryForAdmin.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
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

        public void Dispose()
        {
            _semaphoreSlim?.Dispose();
        }
    }
}