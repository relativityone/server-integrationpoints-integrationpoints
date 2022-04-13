using Relativity.API;
using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.Interfaces.Workspace;
using Relativity.Services.Interfaces.Workspace.Models;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors
{
    internal sealed class WorkspaceNameQuery : IWorkspaceNameQuery
    {
        private readonly IAPILog _logger;

        public WorkspaceNameQuery(IAPILog logger)
        {
            _logger = logger;
        }

        public async Task<string> GetWorkspaceNameAsync(IProxyFactory proxyFactory, int workspaceArtifactId,
            CancellationToken token)
        {
            using (IWorkspaceManager workspaceManager =
                await proxyFactory.CreateProxyAsync<IWorkspaceManager>().ConfigureAwait(false))
            {
                try
                {
                    WorkspaceResponse workspace =
                        await workspaceManager.ReadAsync(workspaceArtifactId).ConfigureAwait(false);

                    if (workspace == null)
                    {
                        _logger.LogError("Couldn't find workspace Artifact ID: {workspaceArtifactId}",
                            workspaceArtifactId);
                        throw new SyncException($"Couldn't find workspace Artifact ID: {workspaceArtifactId}");
                    }

                    return workspace.Name;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to query for workspace Artifact ID: {workspaceArtifactId}",
                        workspaceArtifactId);
                    throw;
                }
            }
        }
    }
}
