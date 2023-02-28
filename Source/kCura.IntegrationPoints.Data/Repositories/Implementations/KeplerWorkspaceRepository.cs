using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Converters;
using kCura.IntegrationPoints.Domain.Models;
using Relativity;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
    public class KeplerWorkspaceRepository : IWorkspaceRepository
    {
        private readonly IAPILog _logger;
        private readonly IServicesMgr _servicesMgr;
        private readonly IRelativityObjectManager _relativityObjectManager;

        public KeplerWorkspaceRepository(IHelper helper, IServicesMgr servicesMgr, IRelativityObjectManager relativityObjectManager)
        {
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<KeplerWorkspaceRepository>();
            _relativityObjectManager = relativityObjectManager;
            _servicesMgr = servicesMgr;
        }

        public WorkspaceDTO Retrieve(int workspaceArtifactId, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
        {
            var query = new QueryRequest()
            {
                ObjectType = new ObjectTypeRef() { ArtifactTypeID = (int)ArtifactType.Case },
                Fields = new List<FieldRef>() { new FieldRef() { Name = WorkspaceFieldsConstants.NAME_FIELD } },
                Condition = $"'ArtifactID' == {workspaceArtifactId}",
            };

            List<RelativityObject> artifactDTOs;
            try
            {
                artifactDTOs = _relativityObjectManager.QueryAsync(query, executionIdentity).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to retrieve workspace {WorkspaceArtifactId}", workspaceArtifactId);
                throw;
            }

            return artifactDTOs.ToWorkspaceDTOs().FirstOrDefault();
        }

        public IEnumerable<WorkspaceDTO> RetrieveAll()
        {
            var query = new QueryRequest()
            {
                ObjectType = new ObjectTypeRef() { ArtifactTypeID = (int)ArtifactType.Case },
                Fields = new List<FieldRef>() { new FieldRef() { Name = WorkspaceFieldsConstants.NAME_FIELD } }
            };

            List<RelativityObject> artifactDTOs;
            try
            {
                artifactDTOs = _relativityObjectManager.QueryAsync(query).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to retrieve workspaces");
                throw;
            }

            return artifactDTOs.ToWorkspaceDTOs();
        }

        public IEnumerable<WorkspaceDTO> RetrieveAllActive()
        {
            using (IWorkspaceManager workspaceManagerProxy = _servicesMgr.CreateProxy<IWorkspaceManager>(ExecutionIdentity.CurrentUser))
            {
                IEnumerable<WorkspaceRef> result = workspaceManagerProxy.RetrieveAllActive().Result;
                return result.ToWorkspaceDTOs();
            }
        }
    }
}
