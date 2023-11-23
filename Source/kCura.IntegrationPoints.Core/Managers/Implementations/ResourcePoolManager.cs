using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;
using Relativity.Services.Exceptions;
using Relativity.Services.Interfaces.Workspace.Models;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
    public class ResourcePoolManager : IResourcePoolManager
    {
        private readonly IResourcePoolRepository _resourcePoolRepository;
        private readonly IServicesMgr _servicesMgr;
        private readonly ILogger<ResourcePoolManager> _logger;

        public ResourcePoolManager(IRepositoryFactory repositoryFactory, IServicesMgr servicesMgr, ILogger<ResourcePoolManager> logger)
        {
            _resourcePoolRepository = repositoryFactory.GetResourcePoolRepository();
            _servicesMgr = servicesMgr;
            _logger = logger;
        }

        public List<ProcessingSourceLocationDTO> GetProcessingSourceLocation(int workspaceId)
        {
            WorkspaceResponse workspace = GetWorkspaceAsync(workspaceId).GetAwaiter().GetResult();

            List<ProcessingSourceLocationDTO> processingSourceLocations =
                _resourcePoolRepository.GetProcessingSourceLocationsByResourcePool(workspace.ResourcePool.Value.ArtifactID);
            return processingSourceLocations;
        }

        protected virtual async Task<WorkspaceResponse> GetWorkspaceAsync(int workspaceId)
        {
            try
            {
                using (var workspaceManager = _servicesMgr.CreateProxy<global::Relativity.Services.Interfaces.Workspace.IWorkspaceManager>(ExecutionIdentity.System))
                {
                    WorkspaceResponse workspaceResponse = await workspaceManager.ReadAsync(workspaceId).ConfigureAwait(false);
                    return workspaceResponse;
                }
            }
            catch (NotFoundException ex)
            {
                LogMissingWorkspaceError(ex, workspaceId);
                throw new ArgumentException($"Cannot find workspace with artifact id: {workspaceId}", ex);
            }
        }

        #region Logging

        private void LogMissingWorkspaceError(NotFoundException ex, int workspaceId)
        {
            _logger.LogError(ex, "Cannot find workspace with artifact id: {WorkspaceId}.", workspaceId);
        }

        #endregion
    }
}
