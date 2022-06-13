using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.UtilityDTO;
using Relativity;
using Relativity.API;
using Relativity.Services.Interfaces.ObjectType;
using Relativity.Services.Interfaces.ObjectType.Models;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Web.Helpers
{
    public class LiquidFormsHelper
    {
        private readonly ConcurrentDictionary<int, bool> _isLiquidFormsDictionary = new ConcurrentDictionary<int, bool>();

        private readonly IServicesMgr _servicesManager;
        private readonly IAPILog _logger;
        private readonly IRelativityObjectManager _relativityObjectManager;

        public LiquidFormsHelper(IServicesMgr servicesManager, IAPILog logger, IRelativityObjectManager relativityObjectManager) 
        {
            _servicesManager = servicesManager;
            _logger = logger;
            _relativityObjectManager = relativityObjectManager;
        }

        public async Task<bool> IsLiquidForms(int workspaceArtifactId)
        {
            if (_isLiquidFormsDictionary.ContainsKey(workspaceArtifactId))
            {
                return _isLiquidFormsDictionary[workspaceArtifactId];
            }

            try
            {
                _logger.LogInformation("Querying for object type Integration Point for workspaceArtifactId - {workspaceArtifactId}", workspaceArtifactId);
                using (IObjectTypeManager objectTypeManager =
                       _servicesManager.CreateProxy<IObjectTypeManager>(ExecutionIdentity.CurrentUser))
                {
                    QueryRequest queryRequest = new QueryRequest
                    {
                        ObjectType = new ObjectTypeRef
                        {
                            ArtifactTypeID = (int)ArtifactType.ObjectType
                        },
                        Condition = $"'Name' == 'Integration Point'"
                    };
                    ResultSet<RelativityObject> queryResult = await _relativityObjectManager.QueryAsync(queryRequest, 0, 1)
                        .ConfigureAwait(false);

                    if (queryResult.Items.Count < 1)
                    {
                        _logger.LogWarning("Integration Point Object Type not found for workspaceArtifactId - {workspaceArtifactId}", workspaceArtifactId);
                        return false;
                    }

                    ObjectTypeResponse result = await objectTypeManager.ReadAsync(workspaceArtifactId,
                        queryResult.Items.First().ArtifactID).ConfigureAwait(false);
                    bool isLiquidForms = result.UseRelativityForms ?? false;
                    _isLiquidFormsDictionary.TryAdd(workspaceArtifactId, isLiquidForms);

                    return isLiquidForms;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Unable to get Integration Point type for workspace Artifact Id - {workspaceArtifactId}. Exception - {ex}", workspaceArtifactId, ex.Message);
            }

            return false;
        }
    }
}
