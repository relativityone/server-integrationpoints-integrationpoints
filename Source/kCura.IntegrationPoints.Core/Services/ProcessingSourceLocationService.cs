using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Extensions;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Services
{
    public class ProcessingSourceLocationService : IProcessingSourceLocationService
    {
        #region Fields
        private readonly IResourcePoolContext _resourcePoolContext;
        private readonly IResourcePoolManager _resourcePoolManager;
        #endregion //Fields


        public ProcessingSourceLocationService(IResourcePoolContext resourcePoolContext,
            IResourcePoolManager resourcePoolManager)
        {
            _resourcePoolContext = resourcePoolContext;
            _resourcePoolManager = resourcePoolManager;
        }

        public bool IsEnabled()
        {
            return _resourcePoolContext.IsProcessingSourceLocationEnabled();
        }

        public bool IsProcessingSourceLocation(string path, int workspaceArtifactId)
        {
            if (!IsEnabled())
            {
                return false;
            }

            List<ProcessingSourceLocationDTO> processingSourceLocations =
                _resourcePoolManager.GetProcessingSourceLocation(workspaceArtifactId);

            return processingSourceLocations.Select(dto => dto.Location).Any(path.IsSubPathOf);
        }
    }
}